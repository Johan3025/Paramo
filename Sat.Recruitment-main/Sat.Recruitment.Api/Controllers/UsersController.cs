using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Sat.Recruitment.Api.Controllers
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Errors { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public partial class UsersController : ControllerBase
    {
        private const string USERDUPLICATED = "The user is Duplicated";
        private const string USERCREATED = "User Created";
        private const int ONEHUNDRED = 100;
        private readonly List<User> _users = new List<User>();
        public UsersController()
        {
        }

        [HttpPost]
        [Route("/create-user")]
        public Result CreateUser(string name, string email, string address, string phone, string userType, string money)
        {
            var errors = "";

            ValidateFields(name, email, address, phone, ref errors);


            if (!string.IsNullOrEmpty(errors))
            {
                return new Result()
                {
                    IsSuccess = false,
                    Errors = errors
                };
            }

            User newUser = CheckingKindOfUserMethod(name, email, address, phone, userType, money);

            var reader = ReadUsersFromFile();

            //Normalize email
            NormalizeEmailMethod(newUser, reader);

            try
            {
                var isDuplicated = false;

                isDuplicated = ValidateDuplicatedMethod(name, email, address, phone);

                if (!isDuplicated)
                {
                    Debug.WriteLine(USERCREATED);

                    return new Result()
                    {
                        IsSuccess = true,
                        Errors = USERCREATED
                    };
                }
                else
                {
                    Debug.WriteLine(USERDUPLICATED);

                    return new Result()
                    {
                        IsSuccess = false,
                        Errors = USERDUPLICATED
                    };
                }
            }
            catch
            {
                Debug.WriteLine(USERDUPLICATED);
                return new Result()
                {
                    IsSuccess = false,
                    Errors = USERDUPLICATED
                };
            }
        }

        private bool ValidateDuplicatedMethod(string name, string email, string address, string phone)
        {
            bool isDuplicated = !(_users.FirstOrDefault(x => x.Email == email || x.Phone == phone) is null);
            if (!(_users.FirstOrDefault(x => x.Name == name || x.Address == address) is null))
            {
                isDuplicated = true;
                throw new Exception(USERDUPLICATED);
            }

            return isDuplicated;
        }

        private static User CheckingKindOfUserMethod(string name, string email, string address, string phone, string userType, string money)
        {
            var newUser = new User
            {
                Name = name,
                Email = email,
                Address = address,
                Phone = phone,
                UserType = userType,
                Money = decimal.Parse(money)
            };

            switch (newUser.UserType)
            {
                case "Normal":
                    NormalUserMethod(money, newUser);
                    break;

                case "SuperUser":
                    SuperUserMethod(money, newUser);
                    break;

                case "Premium":
                    PremiumUserMethod(money, newUser);
                    break;

                default:
                    break;
            }

            return newUser;
        }

        private void NormalizeEmailMethod(User newUser, StreamReader reader)
        {
            var aux = newUser.Email.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

            var atIndex = aux[0].IndexOf("+", StringComparison.Ordinal);

            aux[0] = atIndex < 0 ? aux[0].Replace(".", "") : aux[0].Replace(".", "").Remove(atIndex);

            newUser.Email = string.Join("@", new string[] { aux[0], aux[1] });

            while (reader.Peek() >= 0)
            {
                var line = reader.ReadLineAsync().Result;
                var user = new User
                {
                    Name = line.Split(',')[0].ToString(),
                    Email = line.Split(',')[1].ToString(),
                    Phone = line.Split(',')[2].ToString(),
                    Address = line.Split(',')[3].ToString(),
                    UserType = line.Split(',')[4].ToString(),
                    Money = decimal.Parse(line.Split(',')[5].ToString()),
                };
                _users.Add(user);
            }
            reader.Close();
        }

        private static void PremiumUserMethod(string money, User newUser)
        {
            if (decimal.Parse(money) > ONEHUNDRED)
            {
                var gif = decimal.Parse(money) * 2;
                newUser.Money += gif;
            }
        }

        private static void SuperUserMethod(string money, User newUser)
        {
            if (decimal.Parse(money) > ONEHUNDRED)
            {
                var percentage = Convert.ToDecimal(0.20);
                var gif = decimal.Parse(money) * percentage;
                newUser.Money += gif;
            }
        }

        private static void NormalUserMethod(string money, User newUser)
        {
            if (decimal.Parse(money) > ONEHUNDRED)
            {
                var percentage = Convert.ToDecimal(0.12);
                //If new user is normal and has more than USD100
                var gif = decimal.Parse(money) * percentage;
                newUser.Money += gif;
            }
            if (decimal.Parse(money) < ONEHUNDRED)
            {
                if (decimal.Parse(money) > 10)
                {
                    var percentage = Convert.ToDecimal(0.8);
                    var gif = decimal.Parse(money) * percentage;
                    newUser.Money += gif;
                }
            }
        }

        //Validate errors
        private void ValidateFields(string name, string email, string address, string phone, ref string errors)
        {
            if (name == null)
                //Validate if Name is null
                errors = "The name is required";
            if (email == null)
                //Validate if Email is null
                errors += " The email is required";
            if (address == null)
                //Validate if Address is null
                errors += " The address is required";
            if (phone == null)
                //Validate if Phone is null
                errors += " The phone is required";
        }
    }
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string UserType { get; set; }
        public decimal Money { get; set; }
    }
}
