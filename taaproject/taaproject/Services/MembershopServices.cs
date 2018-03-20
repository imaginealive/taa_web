﻿using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace taaproject.Services
{
    using Microsoft.AspNetCore.Identity.MongoDB;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using taaproject.Models.Membership;
    using taaproject.Services.Interfaces;
    using static taaproject.Services.ProjectService;

    public class MembershopServices : IMembership
    {
        public IMongoClient client;
        public IMongoDatabase database;
        public IServiceConfigurations mongoDB;
        SignInManager<IdentityUser> _SignInManager;
        UserManager<IdentityUser> _UserManager;

        public MembershopServices(
            IConfiguration config,
            IServiceConfigurations mongo,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            mongoDB = mongo;
            client = new MongoClient(mongoDB.DefaultConnection);
            database = client.GetDatabase(mongoDB.DatabaseName);
            _SignInManager = signInManager;
            _UserManager = userManager;
        }

        public async Task<IEnumerable<MembershipInformation>> GetMemberships(string projectid)
        {
            var areDataValidation = !string.IsNullOrEmpty(projectid);
            if (!areDataValidation) return Enumerable.Empty<MembershipInformation>();

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            var project = await project_collection.FindAsync(it => it._id == projectid);
            var selectProject = project.FirstOrDefault();
            var isProjectValid = selectProject != null;
            if (!isProjectValid) return Enumerable.Empty<MembershipInformation>();

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var memberships = await member_collection.FindAsync(it => it.Project_id == selectProject._id);
            var membershipList = memberships.ToList();

            var user_collection = database.GetCollection<IdentityUser>(mongoDB.UserCollection);
            var users = await user_collection.FindAsync(it => true);
            var usersList = users.ToList();

            var result = new List<MembershipInformation>();
            foreach (var item in membershipList)
            {
                var currentUser = usersList.FirstOrDefault(it => it.UserName == item.MemberUserName);
                var email = currentUser != null ? currentUser.Email : "ไม่พบอีเมล์";
                result.Add(new MembershipInformation
                {
                    _id = item._id,
                    Project_id = item.Project_id,
                    MemberUserName = item.MemberUserName,
                    Rank = item.Rank,
                    Work = item.Work,
                    Email = email
                });
            }
            return result.ToList();
        }

        public async Task<MembershipModel> GetMembership(string projectid, string username)
        {
            var areDataValidation = !string.IsNullOrEmpty(projectid) && !string.IsNullOrEmpty(username);
            if (!areDataValidation) return new MembershipModel();

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            var project = await project_collection.FindAsync(it => it._id == projectid);
            var selectProject = project.FirstOrDefault();
            var isProjectValid = selectProject != null;
            if (!isProjectValid) return new MembershipModel();

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var selectMember = await member_collection.FindAsync(it => it.MemberUserName == username && it.Project_id == projectid);
            var result = selectMember.FirstOrDefault();
            return result;
        }

        public async Task<bool> InviteMembership(string projectid, string username)
        {
            var areDataValidation = !string.IsNullOrEmpty(projectid) && !string.IsNullOrEmpty(username);
            if (!areDataValidation) return false;

            var user_collection = database.GetCollection<IdentityUser>(mongoDB.UserCollection);
            var user = await user_collection.FindAsync(it => it.Email == username);
            var selectUser = user.FirstOrDefault();
            var isUserValid = selectUser != null;
            if (!isUserValid) return false;

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            var project = await project_collection.FindAsync(it => it._id == projectid);
            var selectProject = project.FirstOrDefault();
            var isProjectValid = selectProject != null;
            if (!isProjectValid) return false;

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var selectMember = await member_collection.FindAsync(it => it.MemberUserName == selectUser.UserName && it.Project_id == projectid);
            var isMemberAlreadyExist = selectMember.FirstOrDefault() != null;
            if (isMemberAlreadyExist) return false;

            var member = new MembershipModel
            {
                _id = Guid.NewGuid().ToString(),
                Project_id = selectProject._id,
                MemberUserName = selectUser.UserName,
                Rank = ProjectMemberRank.Developer,
                Work = "Unset"
            };
            await member_collection.InsertOneAsync(member);
            return true;
        }

        public async Task<bool> RemoveMembership(string projectid, string username)
        {
            var areDataValidation = !string.IsNullOrEmpty(projectid) && !string.IsNullOrEmpty(username);
            if (!areDataValidation) return false;

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            var project = await project_collection.FindAsync(it => it._id == projectid);
            var selectProject = project.FirstOrDefault();
            var isProjectValid = selectProject != null;
            if (!isProjectValid) return false;

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var members = await member_collection.FindAsync(it => it.MemberUserName == username && it.Project_id == projectid);
            var selectmember = members.FirstOrDefault();
            if (selectmember == null) return false;

            var filter = Builders<MembershipModel>.Filter.Eq(it => it._id, selectmember._id);
            await member_collection.DeleteOneAsync(filter);
            return true;
        }

        public async Task<bool> ChangeMembershipInformation(MembershipModel request)
        {
            var areDataValidation =
                request != null
                && !string.IsNullOrEmpty(request._id)
                && !string.IsNullOrEmpty(request.Project_id)
                && !string.IsNullOrEmpty(request.MemberUserName)
                && !string.IsNullOrEmpty(request.Work)
                && !string.IsNullOrEmpty(request.Rank.ToString());
            if (!areDataValidation) return false;
            
            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            await member_collection.ReplaceOneAsync(it => it._id == request._id, request);
            return true;
        }
    }
}
