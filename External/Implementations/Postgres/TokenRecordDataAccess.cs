﻿using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Npgsql;

namespace CarCareTracker.External.Implementations
{
    public class PGTokenRecordDataAccess : ITokenRecordDataAccess
    {
        private NpgsqlConnection pgDataSource;
        private readonly ILogger<PGTokenRecordDataAccess> _logger;
        private static string tableName = "tokenrecords";
        public PGTokenRecordDataAccess(IConfiguration config, ILogger<PGTokenRecordDataAccess> logger)
        {
            pgDataSource = new NpgsqlConnection(config["POSTGRES_CONNECTION"]);
            _logger = logger;
            try
            {
                pgDataSource.Open();
                //create table if not exist.
                string initCMD = $"CREATE TABLE IF NOT EXISTS app.{tableName} (id INT GENERATED ALWAYS AS IDENTITY primary key, body TEXT not null, emailaddress TEXT not null)";
                using (var ctext = new NpgsqlCommand(initCMD, pgDataSource))
                {
                    ctext.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        public List<Token> GetTokens()
        {
            try
            {
                string cmd = $"SELECT id, emailaddress, body FROM app.{tableName}";
                var results = new List<Token>();
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            Token result = new Token();
                            result.Id = int.Parse(reader["id"].ToString());
                            result.EmailAddress = reader["emailaddress"].ToString();
                            result.Body = reader["body"].ToString();
                            results.Add(result);
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<Token>();
            }
        }
        public Token GetTokenRecordByBody(string tokenBody)
        {
            try
            {
                string cmd = $"SELECT id, emailaddress, body FROM app.{tableName} WHERE body = @body";
                var result = new Token();
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("body", tokenBody);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            result.Id = int.Parse(reader["id"].ToString());
                            result.EmailAddress = reader["emailaddress"].ToString();
                            result.Body = reader["body"].ToString();
                        }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Token();
            }
        }
        public Token GetTokenRecordByEmailAddress(string emailAddress)
        {
            try
            {
                string cmd = $"SELECT id, emailaddress, body FROM app.{tableName} WHERE emailaddress = @emailaddress";
                var result = new Token();
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("emailaddress", emailAddress);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            result.Id = int.Parse(reader["id"].ToString());
                            result.EmailAddress = reader["emailaddress"].ToString();
                            result.Body = reader["body"].ToString();
                        }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Token();
            }
        }
        public bool CreateNewToken(Token token)
        {
            try
            {
                string cmd = $"INSERT INTO app.{tableName} (emailaddress, body) VALUES(@emailaddress, @body) RETURNING id";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("emailaddress", token.EmailAddress);
                    ctext.Parameters.AddWithValue("body", token.Body);
                    token.Id = Convert.ToInt32(ctext.ExecuteScalar());
                    return token.Id != default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        public bool DeleteToken(int tokenId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE id = @id";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("id", tokenId);
                    return ctext.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}