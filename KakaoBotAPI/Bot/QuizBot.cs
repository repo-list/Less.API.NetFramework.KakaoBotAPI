using Less.API.NetFramework.KakaoBotAPI.People;
using Less.API.NetFramework.KakaoBotAPI.Util;
using System;
using System.Collections.Generic;

namespace Less.API.NetFramework.KakaoBotAPI.Bot
{
    public abstract class QuizBot : ChatBot
    {
        public QuizBot(string roomName, Target type, string identifier) : base(roomName, type, identifier) { }

        protected override List<User> LoadUserData()
        {
            var document = GetUserDataDocument();
            List<User> data = new List<User>();

            string nickname;
            int experience, level, money, generation;
            Title currentTitle;
            List<Title> availableTitles;
            string[] tempArray;

            foreach (var node in document.ChildNodes)
            {
                nickname = node.GetValue("nickname");
                experience = Convert.ToInt32(node.GetValue("experience"));
                level = Convert.ToInt32(node.GetValue("level"));
                money = Convert.ToInt32(node.GetValue("money"));
                generation = Convert.ToInt32(node.GetValue("generation"));
                currentTitle = new Title(node.GetValue("currentTitle"));
                availableTitles = new List<Title>();
                tempArray = node.GetValue("availableTitles").Split(',');
                for (int i = 0; i < tempArray.Length; i++) availableTitles.Add(new Title(tempArray[i]));

                data.Add(new QuizUser(nickname, experience, level, money, generation, currentTitle, availableTitles));
            }

            return data;
        }

        protected override void SaveUserData()
        {
            string path = GetUserDataFilePath();

            var helper = new XmlHelper(path);
            var nodeList = new List<XmlHelper.Node>();
            XmlHelper.Node node;
            string temp;

            for (int i = 0; i < Users.Count; i++)
            {
                node = helper.GetNewNode("user");
                node.AddValue("nickname", Users[i].Nickname);
                node.AddValue("experience", (Users[i] as QuizUser).Experience);
                node.AddValue("level", (Users[i] as QuizUser).Level);
                node.AddValue("money", (Users[i] as QuizUser).Money);
                node.AddValue("generation", (Users[i] as QuizUser).Generation);
                node.AddValue("currentTitle", (Users[i] as QuizUser).CurrentTitle.Name);
                temp = "";
                for (int j = 0; j < (Users[i] as QuizUser).AvailableTitles.Count; j++) temp += (Users[i] as QuizUser).AvailableTitles[j].Name + ",";
                node.AddValue("availableTitles", temp.Substring(0, temp.Length - 1));

                nodeList.Add(node);
            }

            helper.CreateFile("list", nodeList);
        }

        protected override User AddNewUser(string userName)
        {
            var user = new QuizUser(userName, QuizUser.NewExperience, QuizUser.NewLevel, QuizUser.NewMoney, QuizUser.NewGeneration, QuizUser.NewCurrentTitle, QuizUser.NewAvailableTitles);
            Users.Add(user);
            SaveUserData();
            return user;
        }
    }
}
