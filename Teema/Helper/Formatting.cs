using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Teema.Helpers {
    public static class Formatting {
        public static string getFormattedUsername(string username) {
            TeemaDBEntities entities = new TeemaDBEntities();
            string formattedUsername;
            if (entities.Users.Where(u => u.Username == username).Count() > 0) {
                User matchedUser = entities.Users.First(u => u.Username == username);
                int UserId = matchedUser.Id;
                int CountryId = matchedUser.CountryId;
                string NameLink = "<a id='username' href='/u/" + username + "'>" + username + "</a>";
                string FlagCode = "<img src='/Content/Images/blank.gif' class='flag flag-"
                        + entities.Countries.First(c => c.Id == CountryId).Code + "' />";
                formattedUsername = NameLink + " " + FlagCode;
            } else {
                formattedUsername = "<a href='#'>non-existing-account</a>";
            }
            return formattedUsername;
        }

        public static string getFormattedDate(DateTime dateTime) {
            string formattedTime;
            TimeSpan timeSpan = DateTime.Now.Subtract(dateTime);
            if (timeSpan.TotalDays >= 2 * 365)
                formattedTime = (int)timeSpan.TotalDays / 365 + " years";
            else if ((int)timeSpan.TotalDays >= 365)
                formattedTime = timeSpan.Days + " year";
            else if ((int)timeSpan.TotalDays > 1)
                formattedTime = timeSpan.Days + " days";
            else if ((int)timeSpan.TotalDays == 1)
                formattedTime = timeSpan.Days + " day";
            else if ((int)timeSpan.TotalHours > 1)
                formattedTime = timeSpan.Hours + " hours";
            else if ((int)timeSpan.TotalHours == 1)
                formattedTime = timeSpan.Hours + " hour";
            else if ((int)timeSpan.TotalMinutes > 1)
                formattedTime = timeSpan.Minutes + " minutes";
            else if ((int)timeSpan.TotalMinutes == 1)
                formattedTime = timeSpan.Minutes + " minute";
            else
                formattedTime = "0 minutes";
            return formattedTime + " ago";
        }
    }
}