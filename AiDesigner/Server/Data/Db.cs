using System;
using System.Data;
using System.Data.Common;
using System.Text;
using AiDesigner.Shared.Data;
using Azure;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NodeBaseApi.Version2;


namespace AiDesigner.Server.Data
{
    public class DBConnection
    {
        private readonly string _connectionString;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        public DBConnection(string connectionString)
        {
            _connectionString = connectionString;

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter> { new TupleConverter(), new BlockJsonConverter() },
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        // Functions to create a program to connect a user to a program
        public async Task<Guid> SaveProgramAsync(ProgramObject programObject)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string programDataJson = JsonConvert.SerializeObject(programObject, _jsonSerializerSettings);

                    var query = @"
                        INSERT INTO Ludde.programs (Id, Name, Description, Author, AuthorName, ApiKey, IsCustomBlock, IsPublic, ProgramData, SupportsSessions, Image, LastOpened )
                        VALUES (@Id, @Name, @Description, @Author, @AuthorName, @ApiKey, @IsCustomBlock, @IsPublic, @ProgramData, @SupportsSessions, @Image, @LastOpened);
                    ";

                    var parameters = new DynamicParameters();
                    parameters.Add("Id", programObject.Id);
                    parameters.Add("Name", programObject.Name);
                    parameters.Add("Description", programObject.Description);
                    parameters.Add("Author", programObject.Author);
                    parameters.Add("AuthorName", programObject.AuthorName);
                    parameters.Add("SupportsSessions", programObject.SupportsSessions); // Add the SupportsSessions parameter

                    if (programObject is CustomProgram program)
                    {
                        parameters.Add("ApiKey", program.ApiKey);
                        parameters.Add("IsCustomBlock", false);
                    }
                    else
                    {
                        parameters.Add("ApiKey", "no key");
                        parameters.Add("IsCustomBlock", true);
                    }

                    parameters.Add("IsPublic", programObject.IsPublic); // Add the IsPublic parameter
                    parameters.Add("ProgramData", programDataJson);
                    if (programObject.Image != null)
                    {
                        parameters.Add("Image", programObject.Image);
                    }
                    else
                    {
                        parameters.Add("Image", new byte[10]);
                    }
                    parameters.Add("LastOpened", programObject.LastOpened);
                    try
                    {
                        await connection.ExecuteAsync(query, parameters);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Optionally, rethrow the exception if you want it to be caught higher up the stack.
                throw;
            }
            return programObject.Id;
        }
        public async Task<ProgramObject> LoadProgramAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT ProgramData, IsCustomBlock
                    FROM Ludde.programs
                    WHERE Id = @Id;
                ";

                var result = await connection.QueryFirstOrDefaultAsync(query, new { Id = id });

                string programDataJson = result.ProgramData;

                if (result.IsCustomBlock)
                {
                    var program = JsonConvert.DeserializeObject<CustomBlockProgram>(programDataJson, _jsonSerializerSettings);
                    return program;
                }
                else
                {
                    var program = JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings);
                    return program;
                }
            }
        }
        public async Task<bool> HasUserConnectionAsync(Guid programId, Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT COUNT(*)
                    FROM Ludde.user_program_connections
                    WHERE ProgramId = @ProgramId AND UserId = @UserId;
                ";

                int count = await connection.ExecuteScalarAsync<int>(query, new { ProgramId = programId, UserId = userId });

                return count > 0;
            }
        }

        public async Task<IEnumerable<ProgramObject>> GetAllUserProgramsAsync(Guid userId, string Name)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // First, try to get any existing programs for the user
                var userProgramsQuery = @"
            SELECT p.ProgramData, p.IsCustomBlock
            FROM Ludde.programs p
            JOIN Ludde.user_program_connections upc ON p.Id = upc.ProgramId
            WHERE upc.UserId = @UserId;
        ";

                var userProgramsResult = await connection.QueryAsync<dynamic>(userProgramsQuery, new { UserId = userId });

                var programs = userProgramsResult.Select(row =>
                {
                    string programDataJson = row.ProgramData;
                    return row.IsCustomBlock
                        ? JsonConvert.DeserializeObject<CustomBlockProgram>(programDataJson, _jsonSerializerSettings) as ProgramObject
                        : JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings) as ProgramObject;
                }).ToList();

                // If the user has no programs, assign starter programs
                if (!programs.Any())
                {
                    var starterPrograms = await GetStarterProgramsAsync();
                    foreach (var starterProgram in starterPrograms)
                    {
                        // Generate a new ID for the copied program
                        starterProgram.Id = Guid.NewGuid();

                        starterProgram.Author = userId.ToString();
                        starterProgram.AuthorName = Name;

                        var newProgramId = await SaveProgramAsync(starterProgram);
                        await ConnectUserToProgramAsync(userId, newProgramId);
                    }

                    programs = starterPrograms.ToList();
                }

                return programs;
            }
        }

        public async Task<IEnumerable<ProgramObject>> GetStarterProgramsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            SELECT ProgramData, IsCustomBlock
            FROM Ludde.StarterPrograms;";

                var result = await connection.QueryAsync<dynamic>(query);

                return result.Select(row =>
                {
                    string programDataJson = row.ProgramData;
                    return row.IsCustomBlock
                        ? JsonConvert.DeserializeObject<CustomBlockProgram>(programDataJson, _jsonSerializerSettings) as ProgramObject
                        : JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings) as ProgramObject;
                }).ToList();
            }
        }

        public async Task<IEnumerable<CustomProgram>> GetAllUserCustomProgramsAsync(Guid userId)
        {
            var query = @"
                SELECT p.ProgramData
                FROM Ludde.programs p
                JOIN Ludde.user_program_connections upc ON p.Id = upc.ProgramId
                WHERE upc.UserId = @UserId AND p.IsCustomBlock = 0;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<string>(query, new { UserId = userId });

            return result.Select(programDataJson =>
                JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings)
            );
        }
        public async Task<IEnumerable<CustomBlockProgram>> GetAllUserCustomBlocksAsync(Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT p.ProgramData
                    FROM Ludde.programs p
                    JOIN Ludde.user_program_connections upc ON p.Id = upc.ProgramId
                    WHERE upc.UserId = @UserId AND p.IsCustomBlock = 1;
                ";

                var result = await connection.QueryAsync<string>(query, new { UserId = userId });

                return result.Select((dynamic row) =>
                {
                    string programDataJson = row.ProgramData;
                    return JsonConvert.DeserializeObject<CustomBlockProgram>(programDataJson, _jsonSerializerSettings);
                });
            }
        }
        public async Task ConnectUserToProgramAsync(Guid userId, Guid programId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    INSERT INTO Ludde.user_program_connections (UserId, ProgramId, IsCustomBlock)
                    VALUES (@UserId, @ProgramId, @IsCustomBlock);
                ";

                await connection.ExecuteAsync(query, new { UserId = userId, ProgramId = programId, IsCustomBlock = false });
            }
        }
        public async Task DisconnectUserFromProgramAsync(Guid userId, Guid programId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    DELETE FROM Ludde.user_program_connections
                    WHERE UserId = @UserId AND ProgramId = @ProgramId;
                ";

                await connection.ExecuteAsync(query, new { UserId = userId, ProgramId = programId });
            }
        }
        public async Task DeleteProgramAsync(Guid userId, Guid Id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    DELETE FROM Ludde.user_program_connections
                    WHERE ProgramId = @Id;

                    DELETE FROM Ludde.programs
                    WHERE Id = @Id AND Author = @userId;

                    DELETE FROM Ludde.Workshop_Article
                    WHERE ProgramId = @Id;

                    DELETE FROM Ludde.User_Article
                    WHERE ArticleId IN (
                        SELECT Id
                        FROM Ludde.Workshop_Article
                        WHERE ProgramId = @Id
                    );
                ";

                await connection.ExecuteAsync(query, new { userId, Id });
            }
        }
        public async Task<IEnumerable<ProgramObject>> SearchPublicProgramsAsync(string searchTerm)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT ProgramData, IsCustomBlock
                    FROM Ludde.programs
                    WHERE IsPublic = 1 AND (Name LIKE @SearchTerm OR Description LIKE @SearchTerm);
                ";

                var result = await connection.QueryAsync(query, new { SearchTerm = $"%{searchTerm}%" });

                return result.Select<dynamic, ProgramObject>(row =>
                {
                    string programDataJson = row.ProgramData;

                    if (row.IsCustomBlock)
                    {
                        return JsonConvert.DeserializeObject<CustomBlockProgram>(programDataJson, _jsonSerializerSettings);
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings);
                    }
                });
            }
        }
        public async Task UpdateProgramAsync(ProgramObject programObject) //make this check if user have permission
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string programDataJson = JsonConvert.SerializeObject(programObject, _jsonSerializerSettings);

                var query = @"
                    UPDATE Ludde.programs
                    SET Name = @Name, Description = @Description, Author = @Author, AuthorName = @AuthorName, ApiKey = @ApiKey, IsCustomBlock = @IsCustomBlock, IsPublic = @IsPublic, ProgramData = @ProgramData, Image = @Image, SupportsSessions = @SupportsSessions, LastOpened = GETDATE(), SupportsChat = @SupportsChat
                    WHERE Id = @Id;
                ";

                var parameters = new DynamicParameters();
                parameters.Add("Id", programObject.Id);
                parameters.Add("Name", programObject.Name);
                parameters.Add("Description", programObject.Description);
                parameters.Add("Author", programObject.Author);
                parameters.Add("AuthorName", programObject.AuthorName);
                parameters.Add("SupportsSessions", programObject.SupportsSessions);
                parameters.Add("SupportsChat", programObject.SupportsChat);


                if (programObject is CustomProgram program)
                {
                    parameters.Add("ApiKey", program.ApiKey);
                    parameters.Add("IsCustomBlock", false);
                }
                else
                {
                    parameters.Add("ApiKey", "no key");
                    parameters.Add("IsCustomBlock", true);
                }

                parameters.Add("IsPublic", programObject.IsPublic);
                parameters.Add("ProgramData", programDataJson);

                if (programObject.Image != null)
                {
                    parameters.Add("Image", programObject.Image);
                }
                else
                {
                    parameters.Add("Image", new byte[10]);
                }

                await connection.ExecuteAsync(query, parameters);
            }
        }

        //Custom block stuff these are very simpel
        public async Task<Guid> SaveCustomBlockAsync(CustomBlockProgram customBlockProgram)
        {
            return await SaveProgramAsync(customBlockProgram);
        }
        public async Task UpdateCustomBlockAsync(CustomBlockProgram customBlockProgram)
        {
            await UpdateProgramAsync(customBlockProgram);
        }
        public async Task<CustomBlockProgram> LoadCustomBlockAsync(Guid id)
        {
            ProgramObject programObject = await LoadProgramAsync(id);
            return programObject as CustomBlockProgram;
        }
        public async Task DeleteCustomBlockAsync(Guid userId, Guid id)
        {
            await DeleteProgramAsync(userId, id);
        }

        //Workshop stuff
        //Search Articles
        public async Task<int> GetArticleCountForSearchAsync(string searchTerm = null, string searchClass = null, string type = null)
        {
            StringBuilder query = new StringBuilder(@"SELECT COUNT(*) FROM Ludde.Workshop_Article a WHERE 1=1 AND Status = 'Accepted'");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm != "")
            {
                query.Append(" AND (a.Name LIKE @SearchTerm OR a.Description LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrEmpty(searchClass) && searchClass != "Home")
            {
                query.Append(" AND a.SearchClass = @SearchClass");
                parameters.Add("SearchClass", searchClass);
            }

            if (!string.IsNullOrEmpty(type) && type != "Any")
            {
                query.Append(" AND a.Type = @Type");
                parameters.Add("Type", type);
            }

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(query.ToString(), parameters);
        }
        public async Task<IEnumerable<WorkshopArticle>> SearchArticlesAsync(int start, int end, string searchTerm = null, string searchClass = null, string type = null)
        {
            StringBuilder query = new StringBuilder(@"
                SELECT a.*, p.IsPublic, p.Image, AVG(CASE WHEN u.Rating > 0 THEN u.Rating END) as Rating
                FROM Ludde.Workshop_Article a
                LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                LEFT JOIN Ludde.programs p ON a.ProgramId = p.Id
                WHERE 1=1 AND a.Status = 'Accepted'"
            );

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query.Append(" AND (a.Name LIKE @SearchTerm OR a.Description LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrEmpty(searchClass) && searchClass != "Home")
            {
                query.Append(" AND a.SearchClass = @SearchClass");
                parameters.Add("SearchClass", searchClass);
            }

            if (!string.IsNullOrEmpty(type) && type != "Any")
            {
                query.Append(" AND a.Type = @Type");
                parameters.Add("Type", type);
            }

            query.Append(@"
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, p.Image, a.Type, a.Created, a.Downloads, p.IsPublic
                ORDER BY a.Created DESC
                OFFSET @Start ROWS
                FETCH NEXT @Count ROWS ONLY;"
            );

            parameters.Add("Start", start);
            parameters.Add("End", end);
            parameters.Add("Count", end - start);  // Assuming you want to fetch (end - start) number of rows

            await using SqlConnection connection = new SqlConnection(_connectionString);
            var result = new List<WorkshopArticle>();  // Fixed initialization
            try
            {
                result = (await connection.QueryAsync<WorkshopArticle>(query.ToString(), parameters)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
        public async Task<IEnumerable<WorkshopArticle>> GetArticlesByAuthorIdAsync(string authorId)
        {
            StringBuilder query = new StringBuilder(@"
        SELECT a.*, p.IsPublic, p.Image, 
            AVG(CASE WHEN u.Rating > 0 THEN u.Rating ELSE NULL END) as Rating
        FROM Ludde.Workshop_Article a
        LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
        LEFT JOIN Ludde.programs p ON a.ProgramId = p.Id
        WHERE a.AuthorId = @AuthorId
        GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, p.Image, a.Type, a.Created, a.Downloads, p.IsPublic
        ORDER BY a.Created DESC;
    ");

            var parameters = new DynamicParameters();
            parameters.Add("AuthorId", authorId);

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                return await connection.QueryAsync<WorkshopArticle>(query.ToString(), parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<WorkshopArticle>();
            }
        }

        //Get most downloaded articles
        public async Task<int> GetCountOfMostDownloadedArticlesAsync()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Ludde.Workshop_Article;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(query);
        }
        public async Task<IEnumerable<WorkshopArticle>> GetMostDownloadedArticlesAsync(int start, int end)
        {
            const string query = @"
            SELECT a.*, AVG(ISNULL(u.Rating, 0)) as AverageRating
            FROM Ludde.Workshop_Article a
            LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
            GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads
            ORDER BY a.Downloads DESC
            OFFSET @Start ROWS
            FETCH NEXT (@End - @Start) ROWS ONLY;
        ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkshopArticle>(query, new { Start = start, End = end });
        }

        //Get top rated articles
        public async Task<int> GetCountOfTopRatedArticlesAsync()
        {
            const string query = @"
                SELECT COUNT(DISTINCT a.Id)
                FROM Ludde.Workshop_Article a
                JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE u.Rating IS NOT NULL;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(query);
        }
        public async Task<IEnumerable<WorkshopArticle>> GetTopRatedArticlesAsync(int start, int end)
        {
            const string query = @"
                SELECT a.*, AVG(u.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE u.Rating IS NOT NULL
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads
                ORDER BY AverageRating DESC, a.Downloads DESC
                OFFSET @Start ROWS
                FETCH NEXT (@End - @Start) ROWS ONLY;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkshopArticle>(query, new { Start = start, End = end });
        }

        //Article home page
        public async Task<IEnumerable<WorkshopArticle>> GetMostPopularArticlesAsync()
        {
            const string query = @"
    SELECT TOP 5 a.*, p.IsPublic, p.Image, 
        AVG(CASE WHEN u.Rating > 0 THEN u.Rating ELSE NULL END) as Rating, 
        COUNT(u.ArticleId) as ReviewCount,
        ((1.0 / (DATEDIFF(day, a.Created, GETDATE()) + 1) * 0.1) + 
        (COALESCE(a.Downloads, 0) * 0.4) + 
        (COALESCE(AVG(ISNULL(u.Rating, 0)), 0) * 0.5)) as PopularityScore
    FROM Ludde.Workshop_Article a
    LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
    LEFT JOIN Ludde.programs p ON a.ProgramId = p.Id
    WHERE a.Status = 'Accepted'
    GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, p.Image, a.Type, a.Created, a.Downloads, p.IsPublic
    ORDER BY PopularityScore DESC
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkshopArticle>(query);
        }

        public async Task<IEnumerable<WorkshopArticle>> GetTopDownloadedArticlesAsync()
        {
            const string query = @"
    SELECT TOP 5 a.*, p.IsPublic, p.Image, 
        AVG(CASE WHEN u.Rating > 0 THEN u.Rating ELSE NULL END) as Rating 
    FROM Ludde.Workshop_Article a
    LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
    LEFT JOIN Ludde.programs p ON a.ProgramId = p.Id
    WHERE a.Status = 'Accepted'
    GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, p.Image, a.Type, a.Created, a.Downloads, p.IsPublic
    ORDER BY a.Downloads DESC
    ";

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                return await connection.QueryAsync<WorkshopArticle>(query);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in GetTopDownloadedArticlesAsync: {e.Message}");
                return new List<WorkshopArticle>();
            }
        }

        public async Task<IEnumerable<WorkshopArticle>> GetNewestArticlesAsync()
        {
            const string query = @"
    SELECT TOP 15 a.*, p.IsPublic, p.Image, 
        AVG(CASE WHEN u.Rating > 0 THEN u.Rating ELSE NULL END) as Rating
    FROM Ludde.Workshop_Article a
    LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
    LEFT JOIN Ludde.programs p ON a.ProgramId = p.Id
    WHERE a.Status = 'Accepted'
    GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, p.Image, a.Type, a.Created, a.Downloads, p.IsPublic
    ORDER BY a.Created DESC
    ";

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                return await connection.QueryAsync<WorkshopArticle>(query);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in GetNewestArticlesAsync: {e.Message}");
                return new List<WorkshopArticle>();
            }
        }

        public async Task<IEnumerable<WorkshopArticle>> Get5ArticlesByAuthorAsync(string authorId)
        {
            StringBuilder query = new StringBuilder(@"
    SELECT TOP 5 a.*, p.IsPublic, p.Image, 
        AVG(CASE WHEN u.Rating > 0 THEN u.Rating ELSE NULL END) as Rating
    FROM Ludde.Workshop_Article a
    LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
    LEFT JOIN Ludde.programs p ON a.ProgramId = p.Id
    WHERE a.AuthorId = @AuthorId
    GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, p.Image, a.Type, a.Created, a.Downloads, p.IsPublic
    ORDER BY a.Created DESC
    ");

            var parameters = new DynamicParameters();
            parameters.Add("AuthorId", authorId);

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                return await connection.QueryAsync<WorkshopArticle>(query.ToString(), parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in GetTopDownloadedArticlesAsync: {e.Message}");
                return new List<WorkshopArticle>();
            }
        }
        //----------------------------------------------------------------------------------------------------------

        //Get article for admin
        public async Task<IEnumerable<WorkshopArticle>> GetPendingArticlesAsync()
        {
            var queryArticles = @"
                SELECT * 
                FROM Ludde.Workshop_Article
                WHERE Status = 'Pending'
                ORDER BY Created ASC;
            ";

            var queryImages = @"
                SELECT * 
                FROM Article_Images
                WHERE ArticleId IN @ArticleIds;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            // First, retrieve the pending articles
            var pendingArticles = (await connection.QueryAsync<WorkshopArticle>(queryArticles)).ToList();

            return pendingArticles;
        }

        //Article handeling
        public async Task<string> SaveArticleAsync(WorkshopArticle article)
        {
            var query = @"
                INSERT INTO Ludde.Workshop_Article (Id, Name, Description, SearchClass, AuthorId, AuthorName, ProgramId, ApiKey, ProgramImage, Type, Created, Downloads, Status)
                VALUES (@Id, @Name, @Description, @SearchClass, @AuthorId, @AuthorName, @ProgramId, @ApiKey, @ProgramImage, @Type, @Created, @Downloads, 'Accepted');
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, article);
            return article.Id;
        }
        public async Task<WorkshopArticle> GetArticleByIdAsync(Guid id)
        {
            const string query = @"
                SELECT a.*, AVG(u.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE a.Id = @Id
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads, a.Status;
            ";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<WorkshopArticle>(query, new { Id = id });
        }
        public async Task<WorkshopArticle> GetArticleByProgramIdAsync(Guid programId)
        {
            const string query = @"
                SELECT a.*, AVG(u.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE a.ProgramId = @ProgramId
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads, a.Status;
            ";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<WorkshopArticle>(query, new { ProgramId = programId });
        }

        public async Task UpdateArticleAsync(WorkshopArticle article)
        {
            var query = @"
                UPDATE Ludde.Workshop_Article 
                SET Name = @Name, Description = @Description, SearchClass = @SearchClass, AuthorId = @AuthorId, AuthorName = @AuthorName, ProgramId = @ProgramId, ApiKey = @ApiKey, ProgramImage = @ProgramImage, Type = @Type, Created = @Created, Downloads = @Downloads, Status = 'Accepted'
                WHERE Id = @Id;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, article);
        }
        public async Task UpdateArticleStatusAsync(string articleId, string status)
        {
            var query = @"
                UPDATE Ludde.Workshop_Article
                SET Status = @Status
                WHERE Id = @Id;
            ";

            var parameters = new
            {
                Id = articleId,
                Status = status
            };

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task DeleteArticleAsync(Guid id)
        {
            const string query = "DELETE FROM Ludde.Workshop_Article WHERE Id = @Id;";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { Id = id });
        }
        public async Task<IEnumerable<WorkshopArticle>> GetArticlesConnectedToUserAsync(string userId)
        {
            const string query = @"
                SELECT a.*, p.IsPublic, u.IsCreator, u.Rating, u.Review, u.IsFavorite, AVG(ua.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                INNER JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                LEFT JOIN Ludde.User_Article ua ON a.Id = ua.ArticleId
                Left JOIN Ludde.programs p ON a.ProgramId = p.Id
                WHERE u.UserId = @UserId
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads, a.Status, u.IsCreator, u.Rating, u.Review, u.IsFavorite, p.IsPublic;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkshopArticle>(query, new { UserId = userId });
        }

        //Article image handeling
        public async Task<string> DeleteArticleImagesAsync(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                return "Invalid Parameters";
            }

            const string query = @"DELETE FROM Ludde.Article_Images WHERE ArticleId = @ArticleId;";

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    ArticleId = articleId
                };

                int rowsAffected = await connection.ExecuteAsync(query, parameters);

                return rowsAffected > 0 ? "Successfully Deleted" : "No Images Found";
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "An error occurred while deleting the article images.");

                return "An error occurred";
            }
        }
        public async Task ConnectArticleToUserAsync(UserArticle userArticle)
        {
            var query = @"
        IF NOT EXISTS (SELECT 1 FROM Ludde.User_Article WHERE UserId = @UserId AND ArticleId = @ArticleId)
        BEGIN
            INSERT INTO Ludde.User_Article (UserId, ArticleId, IsCreator, Rating, Review, IsFavorite)
            VALUES (@UserId, @ArticleId, @IsCreator, @Rating, @Review, @IsFavorite);
            
            -- Increase the downloads count for the article by 1
            UPDATE Ludde.Workshop_Article
            SET Downloads = Downloads + 1
            WHERE Id = @ArticleId;
        END
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new
            {
                UserId = userArticle.UserId,
                ArticleId = userArticle.ArticleId,
                IsCreator = userArticle.IsCreator,
                Rating = userArticle.Rating,
                Review = userArticle.Review,
                IsFavorite = userArticle.IsFavorite
            });
        }

        public async Task UpdateUserArticleAsync2(UserArticle userArticle, string programId)
        {
            var query = @"
        UPDATE Ludde.User_Article 
        SET Rating = @Rating, Review = @Review
        FROM Ludde.User_Article 
        INNER JOIN Ludde.Workshop_Article ON Ludde.User_Article.ArticleId = Ludde.Workshop_Article.Id
        WHERE Ludde.User_Article.UserId = @UserId 
        AND Ludde.Workshop_Article.ProgramId = @ProgramId;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new
                {
                    userArticle.Rating,
                    userArticle.Review,
                    UserId = userArticle.UserId,
                    ProgramId = programId
                });
            }
            catch (Exception ex)
            {
                // Ideally, you'd log this exception to a file or logging service.
                Console.WriteLine(ex.Message);
            }
        }

        public async Task UpdateUserArticleAsync(UserArticle userArticle)
        {
            var query = @"
                UPDATE Ludde.User_Article 
                SET IsCreator = @IsCreator, Rating = @Rating, Review = @Review, IsFavorite = @IsFavorite
                WHERE UserId = @UserId AND ArticleId = @ArticleId;
            ";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, userArticle);
        }
        public async Task<string> RemoveUserArticleAsync(string userId, string articleId)
        {
            var query = @"
        BEGIN TRANSACTION

        DELETE FROM Ludde.User_Article 
        WHERE UserId = @UserId AND ArticleId = @ArticleId;

        -- Decrease the downloads count for the article by 1
        UPDATE Ludde.Workshop_Article
        SET Downloads = CASE WHEN Downloads > 0 THEN Downloads - 1 ELSE 0 END
        WHERE Id = @ArticleId;

        COMMIT
    ";

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    UserId = userId,
                    ArticleId = articleId
                };

                int rowsAffected = await connection.ExecuteAsync(query, parameters);

                return rowsAffected > 0 ? "Successfully Removed" : "Removal Failed";
            }
            catch (Exception ex)
            {
                return "An error occurred";
            }
        }

        //Chat handeling
        public async Task<IEnumerable<CustomProgram>> GetAllProgramsFromUserWorkshopArticlesAsync(string userId)
        {
            const string query = @"
                SELECT p.ProgramData, u.Rating
                FROM programs p
                INNER JOIN Ludde.Workshop_Article a ON p.Id = a.ProgramId
                INNER JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE u.UserId = @UserId
                AND p.IsCustomBlock = 0;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<(string programDataJson, int? rating)>(query, new { UserId = userId });

            return result.Select(t =>
            {
                var customProgram = JsonConvert.DeserializeObject<CustomProgram>(t.programDataJson, _jsonSerializerSettings);
                customProgram.HasReview = t.rating.HasValue && t.rating.Value > 0;
                return customProgram;
            });
        }


        //News article handeling
        public async Task<IEnumerable<NewsArticle>> GetLatestArticlesAsync()
        {
            string query = @"SELECT TOP 4 * FROM Ludde.News_Article 
                     ORDER BY PublishDate DESC";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<NewsArticle>(query);
        }
        public async Task CreateArticleAsync(string id, string title, string content, byte[] imageData)
        {
            string query = @"INSERT INTO Ludde.News_Article (Id, Title, Content, PublishDate, ImageData)
                     VALUES (@Id, @Title, @Content, @PublishDate, @ImageData)";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            parameters.Add("Title", title);
            parameters.Add("Content", content);
            parameters.Add("PublishDate", DateTime.UtcNow); // Assuming you want to set it to the current UTC date-time
            parameters.Add("ImageData", imageData);

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, parameters);
        }

        //Token handeling
        //Token handling
        public async Task<Wallet> EnsureWalletAndRetrieveTokensAsync(Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
                    DECLARE @TokensToAdd INT = 500;
                    DECLARE @BoughtTokens INT = 2000;

                    SELECT @TokensToAdd = CASE SubscriptionTier
                        WHEN 0 THEN 500
                        WHEN 1 THEN 2000
                        WHEN 2 THEN 8000
                        ELSE 500 
                    END FROM Ludde.TokenWallet WHERE UserId = @UserId;

                    IF NOT EXISTS (SELECT 1 FROM Ludde.TokenWallet WHERE UserId = @UserId)
                    BEGIN
                        INSERT INTO Ludde.TokenWallet (Id, UserId, Tokens, LastRefill, BoughtTokens, SubscriptionTier)
                        OUTPUT inserted.Tokens, inserted.BoughtTokens
                        VALUES (NEWID(), @UserId, @TokensToAdd, @Today, @BoughtTokens, 0); -- Use @BoughtTokens here
                    END
                    ELSE
                    BEGIN
                        UPDATE Ludde.TokenWallet
                        SET Tokens = @TokensToAdd, LastRefill = @Today
                        WHERE UserId = @UserId AND LastRefill <> @Today;

                        SELECT Tokens, BoughtTokens, SubscriptionTier, StripeSubscriptionId
                        FROM Ludde.TokenWallet
                        WHERE UserId = @UserId;
                    END
                ";
                var walletData = await connection.QueryFirstOrDefaultAsync<Wallet>(query, new { UserId = userId });
                return walletData ?? new Wallet();
            }
        }


        public async Task<bool> AddBoughtTokensAsync(Guid userId, int tokensToAdd)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    UPDATE Ludde.TokenWallet
                    SET BoughtTokens = BoughtTokens + @TokensToAdd
                    WHERE UserId = @UserId;

                    SELECT CAST(
                        CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END
                    AS BIT);
                ";

                var success = await connection.ExecuteScalarAsync<bool>(query, new
                {
                    UserId = userId,
                    TokensToAdd = tokensToAdd
                });

                return success;
            }
        }

        public async Task<bool> SetSubscriptionTierAsync(Guid userId, int subscriptionTier)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            UPDATE Ludde.TokenWallet
            SET SubscriptionTier = @SubscriptionTier,
                Tokens = CASE @SubscriptionTier
                    WHEN 1 THEN 2000
                    WHEN 2 THEN 8000
                    ELSE 500 
                END
            WHERE UserId = @UserId;

            SELECT CAST(
                CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END
            AS BIT);
        ";

                var success = await connection.ExecuteScalarAsync<bool>(query, new
                {
                    UserId = userId,
                    SubscriptionTier = subscriptionTier
                });

                return success;
            }
        }

        public async Task<bool> SetStripeSubscriptionIdAsync(Guid userId, string stripeSubscriptionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            UPDATE Ludde.TokenWallet
            SET StripeSubscriptionId = @StripeSubscriptionId
            WHERE UserId = @UserId;

            SELECT CAST(
                CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END
            AS BIT);
        ";

                var success = await connection.ExecuteScalarAsync<bool>(query, new
                {
                    UserId = userId,
                    StripeSubscriptionId = stripeSubscriptionId
                });

                return success;
            }
        }

        public async Task<bool> UpdateSubscriptionStateByStripeIdAsync(string stripeSubscriptionId, int subscriptionTier)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            UPDATE Ludde.TokenWallet
            SET SubscriptionTier = @SubscriptionTier,
                Tokens = CASE @SubscriptionTier
                    WHEN 1 THEN 2000
                    WHEN 2 THEN 8000
                    ELSE 500 
                END
            WHERE StripeSubscriptionId = @StripeSubscriptionId;

            SELECT CAST(
                CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END
            AS BIT);
        ";

                var success = await connection.ExecuteScalarAsync<bool>(query, new
                {
                    StripeSubscriptionId = stripeSubscriptionId,
                    SubscriptionTier = subscriptionTier
                });

                return success;
            }
        }

        public async Task UpdateUserTokensAsync(Guid userId, int tokensToDeduct)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Ensure the connection is opened

                    var updateTokensQuery = @"
                DECLARE @Tokens INT;
                DECLARE @BoughtTokens INT;

                SELECT @Tokens = Tokens, @BoughtTokens = BoughtTokens
                FROM Ludde.TokenWallet
                WHERE UserId = @UserId;

                IF (@Tokens >= @TokensToDeduct)
                BEGIN
                    UPDATE Ludde.TokenWallet
                    SET Tokens = @Tokens - @TokensToDeduct
                    WHERE UserId = @UserId;
                END
                ELSE
                BEGIN
                    SET @TokensToDeduct = @TokensToDeduct - @Tokens;
                    UPDATE Ludde.TokenWallet
                    SET Tokens = 0, 
                        BoughtTokens = CASE 
                                         WHEN @BoughtTokens >= @TokensToDeduct THEN @BoughtTokens - @TokensToDeduct
                                         ELSE 0 
                                       END
                    WHERE UserId = @UserId;
                END
            ";

                    await connection.ExecuteAsync(updateTokensQuery, new { UserId = userId, TokensToDeduct = tokensToDeduct });
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Exception occurred: {sqlEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        //Api Calls handeling
        public async Task<IEnumerable<Call>> GetApiCallsAsync(string Key)
        {
            string query = @"SELECT * FROM Ludde.Call WHERE [Api/UserId] = @Key";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Call>(query, new { Key });
        }

        //ApiKey handeling
        public async Task<int> AddApiKeyAsync(string apiKey, string userId, DateTime created, string name)
        {
            var query = @"
                INSERT INTO Ludde.ApiKeys (ApiKey, UserId, Created, Name)
                VALUES (@ApiKey, @UserId, @Created, @Name);
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.ExecuteAsync(query, new { ApiKey = apiKey, UserId = userId, Created = created, Name = name });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
        public async Task<IEnumerable<ApiKey>> GetApiKeysByUserIdAsync(Guid userId)
        {
            var query = @"
                SELECT * FROM Ludde.ApiKeys
                WHERE UserId = @UserId;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QueryAsync<ApiKey>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<int> DeleteApiKeyAsync(string apiKey)
        {
            var query = @"
                DELETE FROM Ludde.ApiKeys
                WHERE ApiKey = @ApiKey;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { ApiKey = apiKey });
        }

        //Call handeling
        public async Task<IEnumerable<Call>> GetLatest100CallsByUserIdAsync(string userId)
        {
            var query = @"
WITH CombinedCalls AS (
    SELECT
        ProgramId,
        [Api/UserId] AS ApiUserId,                        
        IsTest,
        StartTime,
        EndTime,
        Cost
    FROM
        Ludde.Calls
    WHERE
        [Api/UserId] = @UserId

    UNION ALL

    SELECT
        c.ProgramId,
        c.[Api/UserId] AS UserId,                        
        c.IsTest,
        c.StartTime,
        c.EndTime,
        c.Cost
    FROM
        Ludde.Calls c
    INNER JOIN
        Ludde.ApiKeys ak ON c.[Api/UserId] = ak.UserId
    WHERE
        ak.UserId = @UserId
)
SELECT TOP 100
    *
FROM
    CombinedCalls
ORDER BY
    StartTime DESC;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                var retur = await connection.QueryAsync<Call>(query, new { UserId = userId });
                return retur;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<IEnumerable<Call>> GetLatestCallsAsync(int numCalls)
        {
            var query = @"
                WITH RankedCalls AS (
                    SELECT
                        c.ProgramId,
                        c.[Api/UserId] AS ApiUserId,
                        c.IsTest,
                        c.StartTime,
                        c.EndTime,
                        c.Cost,
                        ROW_NUMBER() OVER (ORDER BY c.StartTime DESC) as RowNum
                    FROM
                        Ludde.Calls c
                )
                SELECT
                    *
                FROM
                    RankedCalls
                WHERE
                    RowNum <= @NumCalls;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QueryAsync<Call>(query, new { NumCalls = numCalls });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<AggregatedData>> GetAggregatedDataAsync(string userId, string timeFrame, string programId)
        {
            var query = new StringBuilder();

            // Add common part of the query
            query.AppendLine(@"
                DECLARE @CurrentYear INT = YEAR(GETDATE());
                DECLARE @CurrentMonth INT = MONTH(GETDATE());
                DECLARE @Today DATE = CAST(GETDATE() AS DATE);
                DECLARE @FirstDayOfYear DATE = DATEFROMPARTS(@CurrentYear, 1, 1);
                DECLARE @FirstDayOfMonth DATE = DATEFROMPARTS(@CurrentYear, @CurrentMonth, 1);
                DECLARE @LastDayOfMonth DATE = EOMONTH(@FirstDayOfMonth);
                DECLARE @StartOfWeek DATE = DATEADD(DAY, 1 - DATEPART(WEEKDAY, @Today), @Today);
                DECLARE @EndOfWeek DATE = DATEADD(DAY, 7 - DATEPART(WEEKDAY, @Today), @Today);
            ");

            // Add specific parts based on the time frame
            if (timeFrame == "Month")
                    {
                        query.AppendLine(@"
                ;WITH DateSeries AS (
                    SELECT @FirstDayOfMonth AS DateValue
                    UNION ALL
                    SELECT DATEADD(DAY, 1, DateValue)
                    FROM DateSeries
                    WHERE DateValue < @LastDayOfMonth
                ),
                GroupedCalls AS (
                    SELECT
                        DATEPART(DAY, c.StartTime) AS Day,
                        COUNT(*) AS CallCount,
                        SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                    FROM
                        Ludde.Calls c
                    WHERE
                        c.[Api/UserId] = @UserId
                        AND (c.ProgramId = @ProgramId OR @ProgramId IS NULL OR @ProgramId = 'All')
                        AND c.StartTime >= @FirstDayOfMonth AND c.StartTime < DATEADD(DAY, 1, @LastDayOfMonth)
                    GROUP BY
                        DATEPART(DAY, c.StartTime)
                )
                SELECT
                    @CurrentYear AS Year,
                    @CurrentMonth AS Month,
                    DATEPART(DAY, ds.DateValue) AS Day,  -- Extract day part as integer
                    ISNULL(gc.CallCount, 0) AS CallCount,
                    ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
                FROM
                    DateSeries ds
                    LEFT JOIN GroupedCalls gc ON DATEPART(DAY, ds.DateValue) = gc.Day
                ORDER BY
                    ds.DateValue;
            ");
            }
            else if (timeFrame == "Day")
            {
                query.AppendLine(@"
        ;WITH HourSeries AS (
            SELECT 0 AS HourValue
            UNION ALL
            SELECT HourValue + 1
            FROM HourSeries
            WHERE HourValue < 23
        ),
        GroupedCalls AS (
            SELECT
                DATEPART(HOUR, c.StartTime) AS Hour,
                COUNT(*) AS CallCount,
                SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
            FROM
                Ludde.Calls c
            WHERE
                c.[Api/UserId] = @UserId
                AND (c.ProgramId = @ProgramId OR @ProgramId IS NULL OR @ProgramId = 'All')
                AND c.StartTime >= @Today AND c.StartTime < DATEADD(DAY, 1, @Today)
            GROUP BY
                DATEPART(HOUR, c.StartTime)
        )
        SELECT
            @CurrentYear AS Year,
            @CurrentMonth AS Month,
            NULL AS Day, 
            hs.HourValue AS Hour, 
            ISNULL(gc.CallCount, 0) AS CallCount,
            ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
        FROM
            HourSeries hs
            LEFT JOIN GroupedCalls gc ON hs.HourValue = gc.Hour
        ORDER BY
            hs.HourValue;
    ");
            }
            else if (timeFrame == "Year")
            {
                // Year-specific query
                query.AppendLine(@"
            ;WITH MonthSeries AS (
                SELECT 1 AS MonthValue
                UNION ALL
                SELECT MonthValue + 1
                FROM MonthSeries
                WHERE MonthValue < 12
            ),
            GroupedCalls AS (
                SELECT
                    DATEPART(MONTH, c.StartTime) AS Month,
                    COUNT(*) AS CallCount,
                    SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                FROM
                    Ludde.Calls c
                WHERE
                    c.[Api/UserId] = @UserId
                    AND (c.ProgramId = @ProgramId OR @ProgramId IS NULL OR @ProgramId = 'All')
                    AND c.StartTime >= @FirstDayOfYear AND c.StartTime < DATEADD(YEAR, 1, @FirstDayOfYear)
                GROUP BY
                    DATEPART(MONTH, c.StartTime)
            )
            SELECT
                @CurrentYear AS Year,
                ms.MonthValue AS Month,
                NULL AS Day,
                NULL AS Hour,
                ISNULL(gc.CallCount, 0) AS CallCount,
                ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
            FROM
                MonthSeries ms
                LEFT JOIN GroupedCalls gc ON ms.MonthValue = gc.Month
            ORDER BY
                ms.MonthValue;
        ");
            }
            else if (timeFrame == "Week")
            {
                // Week-specific query
                query.AppendLine(@"
            ;WITH DateSeries AS (
                SELECT @StartOfWeek AS DateValue
                UNION ALL
                SELECT DATEADD(DAY, 1, DateValue)
                FROM DateSeries
                WHERE DateValue < @EndOfWeek
            ),
            GroupedCalls AS (
                SELECT
                    DATEPART(DAY, c.StartTime) AS Day,
                    COUNT(*) AS CallCount,
                    SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                FROM
                    Ludde.Calls c
                WHERE
                    c.[Api/UserId] = @UserId
                    AND (c.ProgramId = @ProgramId OR @ProgramId IS NULL OR @ProgramId = 'All')
                    AND c.StartTime >= @StartOfWeek AND c.StartTime < @EndOfWeek
                GROUP BY
                    DATEPART(DAY, c.StartTime)
            )
            SELECT
                @CurrentYear AS Year,
                @CurrentMonth AS Month,
                DATEPART(DAY, ds.DateValue) AS Day,
                ISNULL(gc.CallCount, 0) AS CallCount,
                ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
            FROM
                DateSeries ds
                LEFT JOIN GroupedCalls gc ON DATEPART(DAY, ds.DateValue) = gc.Day
            ORDER BY
                ds.DateValue;
        ");
            }
            else
            {
                throw new ArgumentException("Invalid time frame");
            }

            // query.AppendLine("OPTION (MAXRECURSION 0);");

            // Execute the query
            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QueryAsync<AggregatedData>(query.ToString(), new { UserId = userId, ProgramId = programId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<AggregatedData>> GetAggregatedDataForAdminAsync(string timeFrame)
        {
            var query = new StringBuilder();

            // Common part of the query remains the same
            query.AppendLine(@"
                DECLARE @CurrentYear INT = YEAR(GETDATE());
                DECLARE @CurrentMonth INT = MONTH(GETDATE());
                DECLARE @Today DATE = CAST(GETDATE() AS DATE);
                DECLARE @FirstDayOfYear DATE = DATEFROMPARTS(@CurrentYear, 1, 1);
                DECLARE @FirstDayOfMonth DATE = DATEFROMPARTS(@CurrentYear, @CurrentMonth, 1);
                DECLARE @LastDayOfMonth DATE = EOMONTH(@FirstDayOfMonth);
                DECLARE @StartOfWeek DATE = DATEADD(DAY, 1 - DATEPART(WEEKDAY, @Today), @Today);
                DECLARE @EndOfWeek DATE = DATEADD(DAY, 7 - DATEPART(WEEKDAY, @Today), @Today);
            ");

            // The specific parts of the query are adjusted to not filter by UserId or ProgramId
            if (timeFrame == "Month")
            {
                query.AppendLine(@"
            ;WITH DateSeries AS (
                SELECT @FirstDayOfMonth AS DateValue
                UNION ALL
                SELECT DATEADD(DAY, 1, DateValue)
                FROM DateSeries
                WHERE DateValue < @LastDayOfMonth
            ),
            GroupedCalls AS (
                SELECT
                    DATEPART(DAY, c.StartTime) AS Day,
                    COUNT(*) AS CallCount,
                    SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                FROM
                    Ludde.Calls c
                WHERE
                    c.StartTime >= @FirstDayOfMonth AND c.StartTime < DATEADD(DAY, 1, @LastDayOfMonth)
                GROUP BY
                    DATEPART(DAY, c.StartTime)
            )
            SELECT
                @CurrentYear AS Year,
                @CurrentMonth AS Month,
                DATEPART(DAY, ds.DateValue) AS Day,
                ISNULL(gc.CallCount, 0) AS CallCount,
                ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
            FROM
                DateSeries ds
                LEFT JOIN GroupedCalls gc ON DATEPART(DAY, ds.DateValue) = gc.Day
            ORDER BY
                ds.DateValue;
        ");
            }
            // Time Frame: Day
            else if (timeFrame == "Day")
            {
                query.AppendLine(@"
            ;WITH HourSeries AS (
                SELECT 0 AS HourValue
                UNION ALL
                SELECT HourValue + 1
                FROM HourSeries
                WHERE HourValue < 23
            ),
            GroupedCalls AS (
                SELECT
                    DATEPART(HOUR, c.StartTime) AS Hour,
                    COUNT(*) AS CallCount,
                    SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                FROM
                    Ludde.Calls c
                WHERE
                    c.StartTime >= @Today AND c.StartTime < DATEADD(DAY, 1, @Today)
                GROUP BY
                    DATEPART(HOUR, c.StartTime)
            )
            SELECT
                @CurrentYear AS Year,
                @CurrentMonth AS Month,
                NULL AS Day, 
                hs.HourValue AS Hour, 
                ISNULL(gc.CallCount, 0) AS CallCount,
                ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
            FROM
                HourSeries hs
                LEFT JOIN GroupedCalls gc ON hs.HourValue = gc.Hour
            ORDER BY
                hs.HourValue;
        ");
            }
            // Time Frame: Year
            else if (timeFrame == "Year")
            {
                query.AppendLine(@"
            ;WITH MonthSeries AS (
                SELECT 1 AS MonthValue
                UNION ALL
                SELECT MonthValue + 1
                FROM MonthSeries
                WHERE MonthValue < 12
            ),
            GroupedCalls AS (
                SELECT
                    DATEPART(MONTH, c.StartTime) AS Month,
                    COUNT(*) AS CallCount,
                    SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                FROM
                    Ludde.Calls c
                WHERE
                    c.StartTime >= @FirstDayOfYear AND c.StartTime < DATEADD(YEAR, 1, @FirstDayOfYear)
                GROUP BY
                    DATEPART(MONTH, c.StartTime)
            )
            SELECT
                @CurrentYear AS Year,
                ms.MonthValue AS Month,
                NULL AS Day,
                NULL AS Hour,
                ISNULL(gc.CallCount, 0) AS CallCount,
                ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
            FROM
                MonthSeries ms
                LEFT JOIN GroupedCalls gc ON ms.MonthValue = gc.Month
            ORDER BY
                ms.MonthValue;
        ");
            }
            // Time Frame: Week
            else if (timeFrame == "Week")
            {
                query.AppendLine(@"
            ;WITH DateSeries AS (
                SELECT @StartOfWeek AS DateValue
                UNION ALL
                SELECT DATEADD(DAY, 1, DateValue)
                FROM DateSeries
                WHERE DateValue < @EndOfWeek
            ),
            GroupedCalls AS (
                SELECT
                    DATEPART(DAY, c.StartTime) AS Day,
                    COUNT(*) AS CallCount,
                    SUM(CAST(ISNULL(c.Cost, '0') AS decimal(18, 2))) AS TotalRevenue
                FROM
                    Ludde.Calls c
                WHERE
                    c.StartTime >= @StartOfWeek AND c.StartTime < @EndOfWeek
                GROUP BY
                    DATEPART(DAY, c.StartTime)
            )
            SELECT
                @CurrentYear AS Year,
                @CurrentMonth AS Month,
                DATEPART(DAY, ds.DateValue) AS Day,
                ISNULL(gc.CallCount, 0) AS CallCount,
                ISNULL(gc.TotalRevenue, 0) AS TotalRevenue
            FROM
                DateSeries ds
                LEFT JOIN GroupedCalls gc ON DATEPART(DAY, ds.DateValue) = gc.Day
            ORDER BY
                ds.DateValue;
        ");
            }

            else
            {
                throw new ArgumentException("Invalid time frame");
            }

            // Execute the query
            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QueryAsync<AggregatedData>(query.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        //Session stuff
        public async Task<Guid> CreateSessionAsync(Session session)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            INSERT INTO Ludde.sessions (SessionId, UserId, ProgramId, Variables, SessionName, CreatedTime, LastEditedTime)
            VALUES (@SessionId, @UserId, @ProgramId, @Variables, @SessionName, GETUTCDATE(), GETUTCDATE());
        ";

                await connection.ExecuteAsync(query, session);
            }

            return Guid.Parse(session.SessionId);
        }

        public async Task<IEnumerable<Session>> GetSessionsAsync(Guid? userId = null, Guid? programId = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var query = @"
                SELECT SessionId, UserId, ProgramId, Variables, SessionName, CreatedTime, LastEditedTime
                FROM Ludde.sessions
                WHERE 1=1
            ";

                    var parameters = new DynamicParameters();

                    if (userId.HasValue)
                    {
                        query += " AND UserId = @UserId";
                        parameters.Add("UserId", userId);
                    }

                    if (programId.HasValue)
                    {
                        query += " AND ProgramId = @ProgramId";
                        parameters.Add("ProgramId", programId);
                    }

                    return await connection.QueryAsync<Session>(query, parameters);
                }
                catch (SqlException sqlEx)
                {
                    // Handle SQL specific exceptions
                    Console.WriteLine($"SQL Error: {sqlEx.Message} \n StackTrace: {sqlEx.StackTrace}");
                    throw;  // Re-throw the exception so the caller is aware something went wrong
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    Console.WriteLine($"Error: {ex.Message} \n StackTrace: {ex.StackTrace}");
                    throw;  // Re-throw the exception so the caller is aware something went wrong
                }
            }
        }

        public async Task<int> GetTotalSessionsByUserAsync(Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var query = @"
                SELECT COUNT(*) 
                FROM Ludde.sessions
                WHERE UserId = @UserId
            ";

                    var parameters = new DynamicParameters();
                    parameters.Add("UserId", userId);

                    return await connection.QuerySingleAsync<int>(query, parameters);
                }
                catch (SqlException sqlEx)
                {
                    // Handle SQL specific exceptions
                    Console.WriteLine($"SQL Error: {sqlEx.Message} \n StackTrace: {sqlEx.StackTrace}");
                    throw;  // Re-throw the exception so the caller is aware something went wrong
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    Console.WriteLine($"Error: {ex.Message} \n StackTrace: {ex.StackTrace}");
                    throw;  // Re-throw the exception so the caller is aware something went wrong
                }
            }
        }

        public async Task<int> DeleteSessionAsync(string sessionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var query = @"
                DELETE FROM Ludde.sessions
                WHERE SessionId = @SessionId;
            ";

                    var parameters = new DynamicParameters();
                    parameters.Add("SessionId", sessionId);

                    var affectedRows = await connection.ExecuteAsync(query, parameters);
                    return affectedRows;  // This will return the number of rows affected by the DELETE statement.
                }
                catch (SqlException sqlEx)
                {
                    // Handle SQL specific exceptions
                    Console.WriteLine($"SQL Error: {sqlEx.Message} \n StackTrace: {sqlEx.StackTrace}");
                    throw;  // Re-throw the exception so the caller is aware something went wrong
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    Console.WriteLine($"Error: {ex.Message} \n StackTrace: {ex.StackTrace}");
                    throw;  // Re-throw the exception so the caller is aware something went wrong
                }
            }
        }

        // tutorial stuff
        public async Task<IEnumerable<Tutorial>> GetUncompletedTutorialsForUserAsync(string userId)
        {
            var query = @"
        SELECT t.*
        FROM Ludde.Tutorials t
        LEFT JOIN UserTutorials ut ON t.TutorialId = ut.TutorialId AND ut.UserId = @UserId
        WHERE ut.UserId IS NULL OR ut.IsCompleted = 0;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.QueryAsync<Tutorial>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<int> MarkTutorialAsCompletedForUserAsync(int tutorialId, string userId)
        {
            var insertQuery = @"
            IF NOT EXISTS (SELECT 1 FROM Ludde.UserTutorials WHERE UserId = @UserId AND TutorialId = @TutorialId)
            BEGIN
                INSERT INTO Ludde.UserTutorials (UserId, TutorialId, IsCompleted, CompletionDate)
                VALUES (@UserId, @TutorialId, 1, GETUTCDATE())
            END
            ELSE
            BEGIN
                UPDATE Ludde.UserTutorials
                SET IsCompleted = 1, CompletionDate = GETUTCDATE()
                WHERE UserId = @UserId AND TutorialId = @TutorialId
            END
        ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.ExecuteAsync(insertQuery, new { UserId = userId, TutorialId = tutorialId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public async Task<IEnumerable<Tutorial>> GetCompletedTutorialsForUserAsync(string userId)
        {
            var query = @"
        SELECT t.*
        FROM Ludde.Tutorials t
        JOIN Ludde.UserTutorials ut ON t.TutorialId = ut.TutorialId
        WHERE ut.UserId = @UserId AND ut.IsCompleted = 1;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.QueryAsync<Tutorial>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<Tutorial>> GetTutorialsWithUserDataAsync(string userId)
        {
            var query = @"
        SELECT 
            t.TutorialId, 
            t.Name, 
            t.Text, 
            t.Image, 
            ut.IsCompleted, 
            ut.CompletionDate
        FROM Ludde.Tutorials t
        LEFT JOIN Ludde.UserTutorials ut ON t.TutorialId = ut.TutorialId AND ut.UserId = @UserId;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QueryAsync<Tutorial>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<int> CreateTutorialAsync(Tutorial tutorial)
        {
            var query = @"
        INSERT INTO Ludde.Tutorials (Name, Text, Image)
        VALUES (@Name, @Text, @Image);
        SELECT SCOPE_IDENTITY();
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                var tutorialId = await connection.ExecuteScalarAsync<int>(query, tutorial);
                return tutorialId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        // This function updates an existing tutorial
        public async Task<bool> UpdateTutorialAsync(Tutorial tutorial)
        {
            var query = @"
        UPDATE Ludde.Tutorials
        SET Name = @Name, Text = @Text, Image = @Image
        WHERE TutorialId = @TutorialId;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                var affectedRows = await connection.ExecuteAsync(query, tutorial);
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // This function deletes a tutorial based on its ID
        public async Task<bool> DeleteTutorialAsync(int tutorialId)
        {
            var query = @"
        DELETE FROM Ludde.Tutorials
        WHERE TutorialId = @TutorialId;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                var affectedRows = await connection.ExecuteAsync(query, new { TutorialId = tutorialId });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // This function retrieves all tutorials
        public async Task<IEnumerable<Tutorial>> GetAllTutorialsAsync()
        {
            var query = @"
        SELECT * FROM Ludde.Tutorials;
    ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.QueryAsync<Tutorial>(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT COUNT(*)
                    FROM Ludde.AspNetUsers;
                ";

                int userCount = await connection.ExecuteScalarAsync<int>(query);

                return userCount;
            }
        }

        public async Task<(int, int)> GetSubscriptionCountsByTierAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var queryTier1 = @"
            SELECT COUNT(*)
            FROM Ludde.TokenWallet
            WHERE SubscriptionTier = 1;
        ";
                var queryTier2 = @"
            SELECT COUNT(*)
            FROM Ludde.TokenWallet
            WHERE SubscriptionTier = 2;
        ";

                int countTier1 = await connection.ExecuteScalarAsync<int>(queryTier1);
                int countTier2 = await connection.ExecuteScalarAsync<int>(queryTier2);

                return (countTier1, countTier2);
            }
        }
    }
}

 