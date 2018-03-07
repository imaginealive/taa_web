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
        SignInManager<IdentityUser> _SignInManager;
        UserManager<IdentityUser> _UserManager;
        public readonly string projectCollection = "projects";
        public readonly string membershipCollection = "memberships";

        public ProjectService(
            IConfiguration config,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            var mongoCon = config.GetConnectionString("DefaultConnection");
            client = new MongoClient(mongoCon);
            database = client.GetDatabase("taa");
            _SignInManager = signInManager;
            _UserManager = userManager;
        }

        public async Task CreateProjectAsync(ProjectModel model, ClaimsPrincipal User)
        {
            await CreateCollectionAsync(projectCollection);
            await CreateCollectionAsync(membershipCollection);

            model._id = Guid.NewGuid().ToString();
            model.CreateDate = DateTime.Now.Date;

            var project_collection = database.GetCollection<ProjectModel>(projectCollection);
            await project_collection.InsertOneAsync(model);

            var member_collection = database.GetCollection<MembershipModel>(membershipCollection);
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
            await CreateCollectionAsync(projectCollection);
            await CreateCollectionAsync(membershipCollection);

            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(membershipCollection);
            var beMember = await member_collection.FindAsync(it => it.MemberUserName == Username);
            var memberList = beMember.ToList();
            if (memberList == null || memberList.Count == 0) return null;

            var admins = await member_collection.FindAsync(it => it.Rank == ProjectMemberRank.Admin);
            var adminList = admins.ToList();
            var project_collection = database.GetCollection<ProjectModel>(projectCollection);
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
            await CreateCollectionAsync(projectCollection);
            await CreateCollectionAsync(membershipCollection);

            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(membershipCollection);
            var beMember = await member_collection.FindAsync(it => it.MemberUserName == Username && it.Project_id == project_id);
            var member = beMember.FirstOrDefault();
            if (member == null) return null;
            var admin = await member_collection.FindAsync(it => it.Rank == ProjectMemberRank.Admin && it.Project_id == project_id);
            var project_collection = database.GetCollection<ProjectModel>(projectCollection);
            var projects = await project_collection.FindAsync(it => !it.DeletedDate.HasValue && it._id == project_id);
            var project = projects.FirstOrDefault();
            if (project == null) return null;
            return new ProjectViewModel
            {
                _id = project._id,
                ProjectName = project.ProjectName,
                ProjectOwner = admin.First().MemberUserName,
                CreateDate = project.CreateDate,
                Description = string.IsNullOrEmpty(project.Description) ? "-" : project.Description
            };
        }

        public async Task DeleteProjectAsync(string project_id, ClaimsPrincipal User)
        {
            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(membershipCollection);
            var beMember = await member_collection.FindAsync(it =>
            it.MemberUserName == Username &&
            it.Project_id == project_id &&
            it.Rank == ProjectMemberRank.Admin);

            var beAdmin = beMember.FirstOrDefault();
            if (beAdmin == null) return;

            var project_collection = database.GetCollection<ProjectModel>(projectCollection);

            await project_collection.FindOneAndUpdateAsync(
                Builders<ProjectModel>.Filter.Eq(it => it._id, project_id),
                Builders<ProjectModel>.Update.Set(it => it.DeletedDate, DateTime.Now.Date)
            );
        }

        private async Task CreateCollectionAsync(string collection_name)
        {
            try
            {
                await database.CreateCollectionAsync(collection_name);
            }
            catch (Exception)
            {
            }
        }

        public class ProjectModel
        {
            public string _id { get; set; }
            [Required(ErrorMessage = "กรุณาใส่ชื่อ Project")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [Display(Name = "Project name")]
            public string ProjectName { get; set; }

            [StringLength(300, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
            [Display(Name = "Description")]
            public string Description { get; set; }
            public DateTime CreateDate { get; set; }

            [DataType(DataType.DateTime)]
            [Display(Name = "Start Project Date")]
            public DateTime StartDate { get; set; }
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
