using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.Data.SqlClient;
using NewExerciseLog.UI.Models;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace NewExerciseLog.UI.Pages.Users
{
    public class NewUserModel : PageModel
    {
        [BindProperty]
        public User NewUser { get; set; } = new User();
        
        public string  ServerSideUsernameError { get; set; } = new string("");

        public void OnGet()
        {
        }

   
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                /*
                             * 1. Create a SQL connection object
                             * 2. Construct a SQL statement
                             * 3. Create a SQL command object
                             * 4. Open the SQL connection
                             * 5. Execute the SQL command
                             * 6. Close the SQL connection
                             * 
                             */
                using (SqlConnection conn = new SqlConnection(DBHelper.GetConnectionString()))
                {
                    String sqlGetUsername = "SELECT UserName FROM [User] WHERE UserName = @userName;";
                    SqlCommand cmdGetUsername = new SqlCommand(sqlGetUsername, conn);
                    cmdGetUsername.Parameters.AddWithValue("@userName", NewUser.UserName);

                    conn.Open();
                    SqlDataReader reader = cmdGetUsername.ExecuteReader();     
                    if (reader.HasRows)
                    {
                        conn.Close();
                        ServerSideUsernameError = "This UserName is already taken";
                        return Page(); //if username is taken
                    }
                    conn.Close();


                    // step 1
                    // step 2
                    string sql = "INSERT INTO [User] (UserName, UserFirstName, UserLastName, UserPasswordHash, Salt, DateJoined, LastLogIn) " +
                        "VALUES (@userName, @firstName, @lastName, @passwordHash, @salt, @joined, @lastLog);";
                    // step 3
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    DateTime date = DateTime.Now;
                    cmd.Parameters.AddWithValue("@userName", NewUser.UserName);
                    cmd.Parameters.AddWithValue("@firstName", NewUser.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", NewUser.LastName);
                    cmd.Parameters.AddWithValue("@passwordHash", NewUser.UserPasswordHash);
                    cmd.Parameters.AddWithValue("@salt", "salt");
                    cmd.Parameters.AddWithValue("@joined", date);
                    cmd.Parameters.AddWithValue("@lastLog", date);
                    conn.Open();
                    cmd.ExecuteNonQuery();

                }




                //now find the ID 
                int newID;
                using (SqlConnection conn = new SqlConnection(DBHelper.GetConnectionString()))
                {
                    string sql = "SELECT UserId FROM [User] WHERE UserName = @userName;";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userName", NewUser.UserName);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        newID = (int)reader["UserId"];
                        
                        //make cookie
                        Credential LoginInfo = new Credential() { UserName = NewUser.UserName, Password = NewUser.UserPasswordHash };
                        if (ModelState.IsValid)
                        {

                            var claims = new List<Claim> {
                                new Claim("UserName", LoginInfo.UserName),
                                new Claim("Password", LoginInfo.Password)
                            };

                            var identity = new ClaimsIdentity(claims, "ExerciseLogCookie");
                            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync("ExerciseLogCookie", principal);

                            return RedirectToPage("HomePage", new { id = newID }); // send em to the home page! (now that we have a cookie)
                        }
                        return Page();
                        //cookie made.
                      
                    }
                }
            }
            return Page();

        }

        public Boolean Test()
        {
            Boolean valid = true;



            return valid;
        }
    }
}


