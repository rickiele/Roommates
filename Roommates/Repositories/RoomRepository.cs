using Microsoft.Data.SqlClient;
using Roommates.Models;
using System.Collections.Generic;
using System.Linq;


namespace Roommates.Repositories
{
    /// <summary>
    ///  This class is responsible for interacting with Room data.
    ///  It inherits from the BaseRepository class so that it can use the BaseRepository's Connection property
    /// </summary>
    public class RoomRepository : BaseRepository
    {
        /// <summary>
        ///  When new RoomRepository is instantiated, pass the connection string along to the BaseRepository
        /// </summary>
        public RoomRepository(string connectionString) : base(connectionString) { }

        // ...We'll add some methods shortly...

        /// <summary>
        ///  Add a new room to the database
        ///   NOTE: This method sends data to the database,
        ///   it does not get anything from the database, so there is nothing to return.
        /// </summary>
        public void Insert(Room room)
        {
            // using is not the same at the top of the code. Completely different.
            // a using inside of a method is saying "I'm going to use some sort of thing
            // that connects to the world out side (make a real connection to a database
            // that is outside in the universe - an external resource - the database
            // This is called a using block.
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Break it down on multiple lines with @
                    // OUTPUT INSERTED.Id is to get the ID back from SQL
                    cmd.CommandText = @"INSERT INTO Room (Name, MaxOccupancy) 
                                         OUTPUT INSERTED.Id 
                                         VALUES (@name, @maxOccupancy)";
                    cmd.Parameters.AddWithValue("@name", room.Name);
                    cmd.Parameters.AddWithValue("@maxOccupancy", room.MaxOccupancy);

                    // ExecteScalar for INSERT instead of ExecuteReader
                    // Scalar is fancy for single value
                    int id = (int)cmd.ExecuteScalar();

                    room.Id = id;
                }
            }
            // At end of using block, it will automatically dispose of that resource
            // Using block is responsible for closing the connection.

            // when this method is finished we can look in the database and see the new room.
        }

        /// <summary>
        ///  Returns a single room with the given id.
        /// </summary>
        // If the id number is not available - like 300 - You will get an empty result set
        public Room GetById(int id)
        {   
            // conn and cmd are common, but it depends on your team
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // @id is a variable. We are using the ID that is passed into use
                    // SQL params are a 2 step process - 1. Set the command text for query
                    // 2. Then set the parameter - @ sign is conventional, and to name it the same as the column
                    // Always use SQL parameters, never use string interpolation and queries - SQL Injection Security bad
                    cmd.CommandText = "SELECT Name, MaxOccupancy FROM Room WHERE Id = @idparam";
                    cmd.Parameters.AddWithValue("@idparam", id);

                    // Part of ADO .NET
                    // Reader is the same kind of thing with one record result or multiple ones - gets a value
                    SqlDataReader reader = cmd.ExecuteReader();

                    // We need somewhere to put this room, so create an object and set to null
                    // In the case that you don't get any results back.
                    Room room = null;

                    // If we only expect a single row back from the database, we don't need a while loop.
                    // If we don't get any results, then return false and close the reader
                    if (reader.Read())
                    {
                 
                        room = new Room
                        {
                            Id = id,

                            // Set value of the property Name of the new room object
                            // GetOrdinal - Get name of the column
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            MaxOccupancy = reader.GetInt32(reader.GetOrdinal("MaxOccupancy")),
                        };
                    }
                    
                    // You need to close the reader yourself.
                    reader.Close();

                    // The value of room will be null because it nevers does the above
                    return room;
                }
            }
        }

        /// <summary>
        ///  Get a list of all Rooms in the database
        /// </summary>
        public List<Room> GetAll()
        {
            //  We must "use" the database connection.
            //  Because a database is a shared resource (other applications may be using it too) we must
            //  be careful about how we interact with it. Specifically, we Open() connections when we need to
            //  interact with the database and we Close() them when we're finished.
            //  In C#, a "using" block ensures we correctly disconnect from a resource even if there is an error.
            //  For database connections, this means the connection will be properly closed.
            using (SqlConnection conn = Connection)
            {
                // Note, we must Open() the connection, the "using" block doesn't do that for us.
                conn.Open();

                // We must "use" commands too.
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    cmd.CommandText = "SELECT Id, Name, MaxOccupancy FROM Room";

                    // Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();

                    // A list to hold the rooms we retrieve from the database.
                    List<Room> rooms = new List<Room>();

                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        // The "ordinal" is the numeric position of the column in the query results.
                        //  For our query, "Id" has an ordinal value of 0 and "Name" is 1.
                        int idColumnPosition = reader.GetOrdinal("Id");

                        // We user the reader's GetXXX methods to get the value for a particular ordinal.
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColumnPosition = reader.GetOrdinal("Name");
                        string nameValue = reader.GetString(nameColumnPosition);

                        int maxOccupancyColumPosition = reader.GetOrdinal("MaxOccupancy");
                        int maxOccupancy = reader.GetInt32(maxOccupancyColumPosition);

                        // Now let's create a new room object using the data from the database.
                        Room room = new Room
                        {
                            Id = idValue,
                            Name = nameValue,
                            MaxOccupancy = maxOccupancy,
                        };

                        // ...and add that room object to our list.
                        rooms.Add(room);
                    }

                    // We should Close() the reader. Unfortunately, a "using" block won't work here.
                    reader.Close();

                    // Return the list of rooms who whomever called this method.
                    return rooms;
                }

                
            }
        }


        /// <summary>
        ///  Updates the room
        /// </summary>
        
        // UPDATE object assumes your room already has an id
        // You will need to update everything all at once (what is PATCH HTTP method?)
        // ExecuteNonQuery
        // You need a where clause or else it will update them all
        public void Update(Room room)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Room
                                    SET Name = @name,
                                        MaxOccupancy = @maxOccupancy
                                    WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@name", room.Name);
                    cmd.Parameters.AddWithValue("@maxOccupancy", room.MaxOccupancy);
                    cmd.Parameters.AddWithValue("@id", room.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///  Delete the room with the given id
        /// </summary>
        // DELETE - you need a where clause or else you will delete it all
        // Cascading delete - manually do that inside a method 
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // What do you think this code will do if there is a roommate in the room we're deleting???
                    cmd.CommandText = "DELETE FROM Room WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }


}