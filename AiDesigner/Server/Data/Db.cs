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
                        INSERT INTO Ludde.programs (Id, Name, Description, Author, AuthorName, ApiKey, IsCustomBlock, IsPublic, ProgramData, SupportsSessions, Image)
                        VALUES (@Id, @Name, @Description, @Author, @AuthorName, @ApiKey, @IsCustomBlock, @IsPublic, @ProgramData, @SupportsSessions, @Image);
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

                    await connection.ExecuteAsync(query, parameters);
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
        public async Task DeleteProgramAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                DELETE FROM Ludde.programs
                WHERE Id = @Id;
            ";

                await connection.ExecuteAsync(query, new { Id = id });
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
                    SET Name = @Name, Description = @Description, Author = @Author, AuthorName = @AuthorName, ApiKey = @ApiKey, IsCustomBlock = @IsCustomBlock, IsPublic = @IsPublic, ProgramData = @ProgramData, Image = @Image, SupportsSessions = @SupportsSessions
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
        public async Task DeleteCustomBlockAsync(Guid id)
        {
            await DeleteProgramAsync(id);
        }

        //Workshop stuff
        //Search Articles
        public async Task<int> GetArticleCountForSearchAsync(string searchTerm = null, string searchClass = null, string type = null)
        {
            StringBuilder query = new StringBuilder(@"SELECT COUNT(*) FROM Ludde.Workshop_Article a WHERE 1=1");

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
                WHERE 1=1"
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
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads
                ORDER BY a.Created DESC
                OFFSET @Start ROWS
                FETCH NEXT (@End - @Start) ROWS ONLY;"
            );

            parameters.Add("Start", start);
            parameters.Add("End", end);

            await using SqlConnection connection = new SqlConnection(_connectionString);

            var result = await connection.QueryAsync<WorkshopArticle>(query.ToString(), parameters);

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

        //Article handeling
        public async Task<string> SaveArticleAsync(WorkshopArticle article)
        {
            var query = @"
            INSERT INTO Ludde.Workshop_Article (Id, Name, Description, SearchClass, AuthorId, AuthorName, ProgramId, ApiKey, ProgramImage, Type, Created, Downloads)
            VALUES (@Id, @Name, @Description, @SearchClass, @AuthorId, @AuthorName, @ProgramId, @ApiKey, @ProgramImage, @Type, @Created, @Downloads);
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
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads;
            ";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<WorkshopArticle>(query, new { Id = id });
        }

        public async Task<IEnumerable<WorkshopArticle>> GetArticlesByAuthorAsync(string authorId)
        {
            const string query = @"
                SELECT a.*, AVG(u.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                LEFT JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                WHERE a.AuthorId = @AuthorId
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads;
            ";
            await using SqlConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkshopArticle>(query, new { AuthorId = authorId });
        }

        public async Task UpdateArticleAsync(WorkshopArticle article)
        {
            var query = @"
                UPDATE Ludde.Workshop_Article 
                SET Name = @Name, Description = @Description, SearchClass = @SearchClass, AuthorId = @AuthorId, AuthorName = @AuthorName, ProgramId = @ProgramId, ApiKey = @ApiKey, ProgramImage = @ProgramImage, Type = @Type, Created = @Created, Downloads = @Downloads
                WHERE Id = @Id;
            ";

            await using SqlConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, article);
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
                SELECT a.*, u.IsCreator, u.Rating, u.Review, u.IsFavorite, AVG(ua.Rating) as AverageRating
                FROM Ludde.Workshop_Article a
                INNER JOIN Ludde.User_Article u ON a.Id = u.ArticleId
                LEFT JOIN Ludde.User_Article ua ON a.Id = ua.ArticleId
                WHERE u.UserId = @UserId
                GROUP BY a.Id, a.Name, a.Description, a.SearchClass, a.AuthorId, a.AuthorName, a.ProgramId, a.ApiKey, a.ProgramImage, a.Type, a.Created, a.Downloads, u.IsCreator, u.Rating, u.Review, u.IsFavorite;
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
            string query = @"SELECT TOP 10 * FROM Ludde.News_Article 
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


    }
}
 