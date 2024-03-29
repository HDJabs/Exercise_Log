using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata;
using Microsoft.Data.SqlClient;
using NewExerciseLog.UI.Models;
using Microsoft.AspNetCore.Authorization;

namespace NewExerciseLog.UI.Pages.Users
{
    [Authorize]
    public class HomePageModel : PageModel
    {
   
        public List<ExerciseGoal> goals { get; set; } = new List<ExerciseGoal>();
        public int ID { get; set; }

        private static int userId;

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public IActionResult OnGet(int id)
        {
            userId = id;
            ID = id;

            //check cookie for credentials
            //compare incoming id (above) to the id in the cookie
            if (!HttpContext.User.HasClaim("Id", id.ToString()))
            {
                return RedirectToPage("/Users/Login");
            }
   





            //form list of goals
            using (SqlConnection conn = new SqlConnection(DBHelper.GetConnectionString()))
            {
				string sql = "SELECT * FROM ExerciseGoal " +
			    "INNER JOIN Exercise ON ExerciseGoal.ExerciseId = Exercise.ExerciseId " +
			    "WHERE UserId=@UserId";
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("@UserId", id);
				conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ExerciseGoal goal = new ExerciseGoal();
                        goal.Goal = reader["Goal"].ToString();
                        goal.ExerciseName = reader["ExerciseName"].ToString();
                        goal.Total = reader["Total"].ToString();
                        goal.ExerciseId = (int)reader["ExerciseId"];
                        goal.ExerciseGoalId = (int)reader["ExerciseGoalId"];

						int goalMinutes = (Int32.Parse(goal.Goal.Substring(0, 2)) * 60) + Int32.Parse(goal.Goal.Substring(3, 2));
						int totalMinutes = (Int32.Parse(goal.Total.Substring(0, 2)) * 60) + Int32.Parse(goal.Total.Substring(3, 2));
						if (totalMinutes > 0)
                            goal.Percent = Math.Round(100*(double)totalMinutes/goalMinutes, 2);
                        else
                            goal.Percent = 0;


                        goals.Add(goal);
                    }
                }
            }

			//find the name
			using (SqlConnection conn = new SqlConnection(DBHelper.GetConnectionString()))
			{
				String sql = "SELECT UserFirstName, UserLastName FROM [User] " +
			    "WHERE UserId=@UserId ;";
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("@UserId", id);
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
                    reader.Read();
                    UserFirstName = reader["UserFirstName"].ToString();
					UserLastName = reader["UserLastName"].ToString();
				}
			}

            return Page();
        }



        public IActionResult OnPost(int id, int exerciseGoalId)
        {
			using (SqlConnection conn = new SqlConnection(DBHelper.GetConnectionString()))
			{
				// step 1
				// step 2
				string sql = "DELETE FROM ExerciseGoal WHERE ExerciseGoalId=@exerciseGoalId;";
				// step 3
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("@exerciseGoalId", exerciseGoalId);

				conn.Open();
				cmd.ExecuteNonQuery();
			}
			return RedirectToPage("HomePage", new { id = userId});
		}
       
    }
}
