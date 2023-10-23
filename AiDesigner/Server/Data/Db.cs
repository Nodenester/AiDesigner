using System;
using System.Data;
using System.Text;
using AiDesigner.Shared.Data;
using Azure;
using Dapper;
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
                    FROM user_program_connections
                    WHERE ProgramId = @ProgramId AND UserId = @UserId;
                ";

                int count = await connection.ExecuteScalarAsync<int>(query, new { ProgramId = programId, UserId = userId });

                return count > 0;
            }
        }

        public async Task<IEnumerable<ProgramObject>> GetAllUserProgramsAsync(Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT p.ProgramData, p.IsCustomBlock
                    FROM Ludde.programs p
                    JOIN Ludde.user_program_connections upc ON p.Id = upc.ProgramId
                    WHERE upc.UserId = @UserId;
                ";

                var result = await connection.QueryAsync<dynamic>(query, new { UserId = userId });

                return result.Select(row =>
                {
                    string programDataJson = row.ProgramData;

                    if (row.IsCustomBlock)
                    {
                        return JsonConvert.DeserializeObject<CustomBlockProgram>(programDataJson, _jsonSerializerSettings) as ProgramObject;
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings) as ProgramObject;
                    }
                });
            }
        }
        public async Task<IEnumerable<CustomProgram>> GetAllUserCustomProgramsAsync(Guid userId)
        {
            var query = @"
                SELECT p.ProgramData
                FROM programs p
                JOIN user_program_connections upc ON p.Id = upc.ProgramId
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
                    FROM programs p
                    JOIN user_program_connections upc ON p.Id = upc.ProgramId
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
                    SET Name = @Name, Description = @Description, Author = @Author, AuthorName = @AuthorName, ApiKey = @ApiKey, IsCustomBlock = @IsCustomBlock, IsPublic = @IsPublic, ProgramData = @ProgramData, Image = @Image, SupportsSessions = @SupportsSessions, LastOpened = GETDATE()
                    WHERE Id = @Id;
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
        SELECT a.*, AVG(ISNULL(u.Rating, 0)) as AverageRating
        FROM Ludde.Workshop_Article a
        LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
        WHERE 1=1 AND Status = 'Accepted'"
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
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.Status, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads
                ORDER BY a.Created DESC
                OFFSET @Start ROWS
                FETCH NEXT (@End - @Start) ROWS ONLY;"
            );

            parameters.Add("Start", start);
            parameters.Add("End", end);

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

            // If there are any pending articles, retrieve the associated images
            if (pendingArticles.Any())
            {
                var articleIds = pendingArticles.Select(a => a.Id);

                List<ArticleImages>  imageLists = new List<ArticleImages>();
                try
                {
                    imageLists = (await connection.QueryAsync<ArticleImages>(queryImages, new { ArticleIds = articleIds })).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                // Group the images by article ID
                var imageGroups = imageLists.GroupBy(img => img.ArticleId);

                // Assign the images to the corresponding articles
                foreach (var group in imageGroups)
                {
                    var article = pendingArticles.Find(a => a.Id == group.Key);
                    if (article != null)
                    {
                        article.ArticleImages = group.ToList();
                    }
                }
            }

            return pendingArticles;
        }

        //Article handeling
        public async Task<string> SaveArticleAsync(WorkshopArticle article)
        {
            var query = @"
                INSERT INTO Ludde.Workshop_Article (Id, Name, Description, SearchClass, AuthorId, AuthorName, ProgramId, ApiKey, ProgramImage, Type, Created, Downloads, Status)
                VALUES (@Id, @Name, @Description, @SearchClass, @AuthorId, @AuthorName, @ProgramId, @ApiKey, @ProgramImage, @Type, @Created, @Downloads, 'Pending');
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
        public async Task<IEnumerable<WorkshopArticle>> GetArticlesByAuthorAsync(string authorId)
        {
            const string query = @"
                SELECT a.*, AVG(u.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE a.AuthorId = @AuthorId
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads, a.Status;
            ";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkshopArticle>(query, new { AuthorId = authorId });
        }
        public async Task UpdateArticleAsync(WorkshopArticle article)
        {
            var query = @"
                UPDATE Ludde.Workshop_Article 
                SET Name = @Name, Description = @Description, SearchClass = @SearchClass, AuthorId = @AuthorId, AuthorName = @AuthorName, ProgramId = @ProgramId, ApiKey = @ApiKey, ProgramImage = @ProgramImage, Type = @Type, Created = @Created, Downloads = @Downloads, Status = 'Pending'
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
        public async Task<string> InsertArticleImageAsync(ArticleImages articleImages)
        {
            if (articleImages == null || string.IsNullOrEmpty(articleImages.ImageId) || string.IsNullOrEmpty(articleImages.ArticleId))
            {
                return "Invalid Parameters";
            }

            const string query = @"INSERT INTO Ludde.Article_Images (ImageId, ArticleId, ImageData, Description) 
                                VALUES (@ImageId, @ArticleId, @ImageData, @Description);";

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    ImageId = articleImages.ImageId,
                    ArticleId = articleImages.ArticleId,
                    ImageData = articleImages.ImageData,
                    Description = articleImages.Description
                };

                int rowsAffected = await connection.ExecuteAsync(query, parameters);

                return rowsAffected > 0 ? "Successfully Inserted" : "Insert Failed";
            }
            catch (Exception ex)
            {
                // Log the exception
                // For example: _logger.LogError(ex, "An error occurred while inserting the article image.");

                return "An error occurred";
            }
        }
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
        public async Task<IEnumerable<ArticleImages>> GetArticleImagesAsync(string articleId)
        {
            var query = @"
                SELECT ai.ImageId, ai.ImageData, ai.Description
                FROM Ludde.Article_Images ai
                WHERE ai.ArticleId = @ArticleId";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<ArticleImages>(query, new { ArticleId = articleId });
        }
        public async Task ConnectArticleToUserAsync(UserArticle userArticle)
        {
            var query = @"
                IF NOT EXISTS (SELECT 1 FROM Ludde.User_Article WHERE UserId = @UserId AND ArticleId = @ArticleId)
                BEGIN
                    INSERT INTO Ludde.User_Article (UserId, ArticleId, IsCreator, Rating, Review, IsFavorite)
                    VALUES (@UserId, @ArticleId, @IsCreator, @Rating, @Review, @IsFavorite);
                END
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { UserId = userArticle.UserId, ArticleId = userArticle.ArticleId, IsCreator = userArticle.IsCreator, Rating = userArticle.Rating, Review = userArticle.Review, IsFavorite = userArticle.IsFavorite });
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
                DELETE FROM Ludde.User_Article 
                WHERE UserId = @UserId AND ArticleId = @ArticleId;
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
                SELECT p.ProgramData
                FROM programs p
                WHERE p.Id IN (
                    SELECT a.ProgramId
                    FROM Ludde.Workshop_Article a
                    INNER JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                    WHERE u.UserId = @UserId
                )
                AND p.IsCustomBlock = 0;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<string>(query, new { UserId = userId });

            return result.Select(programDataJson =>
                JsonConvert.DeserializeObject<CustomProgram>(programDataJson, _jsonSerializerSettings)
            );
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
        public async Task<int> EnsureWalletAndRetrieveTokensAsync(Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM Ludde.TokenWallet WHERE UserId = @UserId)
                    BEGIN
                        INSERT INTO Ludde.TokenWallet (Id, UserId, Tokens)
                        OUTPUT inserted.Tokens
                        VALUES (NEWID(), @UserId, 0);
                    END
                    ELSE
                    BEGIN
                        SELECT Tokens
                        FROM Ludde.TokenWallet
                        WHERE UserId = @UserId;
                    END
                ";
                return await connection.ExecuteScalarAsync<int>(query, new { UserId = userId });
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
        public async Task<int> DeleteApiKeyAsync(string apiKey, string userId)
        {
            var query = @"
                DELETE FROM Ludde.ApiKeys
                WHERE ApiKey = @ApiKey AND UserId = @UserId;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { ApiKey = apiKey, UserId = userId });
        }

        //Call handeling
        public async Task<IEnumerable<Call>> GetLatest100CallsByUserIdAsync(string userId)
        {
            var query = @"
                WITH UserCalls AS (
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
                    JOIN
                        Ludde.ApiKeys ak
                    ON
                        c.[Api/UserId] = ak.UserId
                    WHERE
                        ak.UserId = @UserId
                )
                SELECT
                    *
                FROM
                    UserCalls
                WHERE
                    RowNum <= 200;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);

            try
            {
                return await connection.QueryAsync<Call>(query, new { UserId = userId });
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


    }
}

 