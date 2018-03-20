using Microsoft.Extensions.Configuration;
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
    using taaproject.Models.HomeViewModels;

    public class ProjectService
    {
        public IMongoClient client;
        public IMongoDatabase database;
        public IServiceConfigurations mongoDB;
        SignInManager<IdentityUser> _SignInManager;
        UserManager<IdentityUser> _UserManager;

        public ProjectService(
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

        public async Task CreateProjectAsync(ProjectModel model, ClaimsPrincipal User)
        {
            model._id = Guid.NewGuid().ToString();
            model.CreateDate = DateTime.Now.Date;

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            await project_collection.InsertOneAsync(model);

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var member = new MembershipModel
            {
                _id = Guid.NewGuid().ToString(),
                Project_id = model._id,
                MemberUserName = _UserManager.GetUserName(User),
                Work = "Unset",
                Rank = ProjectMemberRank.Admin
            };
            await member_collection.InsertOneAsync(member);
        }

        public async Task<IEnumerable<ProjectViewModel>> GetAllAllowProjectAsync(ClaimsPrincipal User)
        {
            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var beMember = await member_collection.FindAsync(it => it.MemberUserName == Username);
            var memberList = beMember.ToList();
            if (memberList == null || memberList.Count == 0) return null;

            var admins = await member_collection.FindAsync(it => it.Rank == ProjectMemberRank.Admin);
            var adminList = admins.ToList();
            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            var projects = await project_collection.FindAsync(it => !it.DeletedDate.HasValue);
            return projects.ToList().Where(it => memberList.Any(mb => mb.Project_id == it._id)).Select(pj =>
            {
                return new ProjectViewModel
                {
                    _id = pj._id,
                    ProjectName = pj.ProjectName,
                    ProjectOwner = adminList.First(it => it.Project_id == pj._id).MemberUserName,
                    CreateDate = pj.CreateDate,
                    Description = string.IsNullOrEmpty(pj.Description) ? "-" : pj.Description
                };
            });
        }

        public async Task<ProjectViewModel> GetAllowProjectAsync(string project_id, ClaimsPrincipal User)
        {
            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var beMember = await member_collection.FindAsync(it => it.MemberUserName == Username && it.Project_id == project_id);
            var member = beMember.FirstOrDefault();
            if (member == null) return null;
            var admin = await member_collection.FindAsync(it => it.Rank == ProjectMemberRank.Admin && it.Project_id == project_id);
            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);
            var projects = await project_collection.FindAsync(it => !it.DeletedDate.HasValue && it._id == project_id);
            var project = projects.FirstOrDefault();
            if (project == null) return null;
            return new ProjectViewModel
            {
                _id = project._id,
                ProjectName = project.ProjectName,
                ProjectOwner = admin.First().MemberUserName,
                CreateDate = project.CreateDate,
                StartDate = project.StartDate,
                FinishDate = project.FinishDate,
                Description = string.IsNullOrEmpty(project.Description) ? "-" : project.Description
            };
        }

        public async Task UpdateProjectAsync(ProjectModel model, ClaimsPrincipal User)
        {
            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var beMember = await member_collection.FindAsync(it =>
            it.MemberUserName == Username &&
            it.Project_id == model._id &&
            (it.Rank == ProjectMemberRank.Admin
            || it.Rank == ProjectMemberRank.Master));

            var canUpdate = beMember.FirstOrDefault();
            if (canUpdate == null) return;

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);

            await project_collection.FindOneAndUpdateAsync(
                Builders<ProjectModel>.Filter.Eq(it => it._id, model._id),
                Builders<ProjectModel>.Update
                .Set(it => it.ProjectName, model.ProjectName)
                .Set(it => it.Description, model.Description)
                .Set(it => it.StartDate, model.StartDate)
                .Set(it => it.FinishDate, model.FinishDate)

            );
        }

        public async Task DeleteProjectAsync(string project_id, ClaimsPrincipal User)
        {
            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(mongoDB.MembershipCollection);
            var beMember = await member_collection.FindAsync(it =>
            it.MemberUserName == Username &&
            it.Project_id == project_id &&
            it.Rank == ProjectMemberRank.Admin);

            var beAdmin = beMember.FirstOrDefault();
            if (beAdmin == null) return;

            var project_collection = database.GetCollection<ProjectModel>(mongoDB.ProjectCollection);

            await project_collection.FindOneAndUpdateAsync(
                Builders<ProjectModel>.Filter.Eq(it => it._id, project_id),
                Builders<ProjectModel>.Update.Set(it => it.DeletedDate, DateTime.Now.Date)
            );
        }

        public class ProjectModel
        {
            public string _id { get; set; }
            [Required(ErrorMessage = "กรุณาใส่ชื่อ Project")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [Display(Name = "Project name")]
            public string ProjectName { get; set; }
            
            [Display(Name = "Description")]
            public string Description { get; set; }
            public DateTime CreateDate { get; set; }

            [DataType(DataType.DateTime)]
            [Display(Name = "Start Project Date")]
            public DateTime StartDate { get; set; }

            [DataType(DataType.DateTime)]
            [Display(Name = "Finish Project Date")]
            public DateTime? FinishDate { get; set; }
            public DateTime? DeletedDate { get; set; }
        }

        public class MembershipModel
        {
            public string _id { get; set; }
            public string Project_id { get; set; }
            public string MemberUserName { get; set; }
            public string Work { get; set; }
            public ProjectMemberRank Rank { get; set; }
        }

        public enum ProjectMemberRank
        {
            Admin,
            Master,
            Developer
        }
    }
}
