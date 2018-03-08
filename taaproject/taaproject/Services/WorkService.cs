using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace taaproject.Services
{
    using Microsoft.AspNetCore.Identity.MongoDB;
    using taaproject.Models.WorkViewModels;
    using static taaproject.Services.ProjectService;

    public class WorkService
    {
        public IMongoClient client;
        public IMongoDatabase database;
        SignInManager<IdentityUser> _SignInManager;
        UserManager<IdentityUser> _UserManager;
        public readonly string featureCollection = "features";
        public readonly string storyCollection = "stories";
        public readonly string taskCollection = "tasks";
        public readonly string membershipCollection = "memberships";

        public WorkService(
            IConfiguration config,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            var mongoCon = config.GetConnectionString("DefaultConnection");
            client = new MongoClient(mongoCon);
            database = client.GetDatabase("taa");

            _UserManager = userManager;
            _SignInManager = signInManager;
        }

        public async Task<IEnumerable<FeatureViewModel>> GetAllAllowWorkAsync(string project_id, ClaimsPrincipal User)
        {
            await CreateCollectionAsync(featureCollection);
            await CreateCollectionAsync(storyCollection);
            await CreateCollectionAsync(taskCollection);

            var Username = _UserManager.GetUserName(User);

            var member_collection = database.GetCollection<MembershipModel>(membershipCollection);
            var beMember = await member_collection.FindAsync(it => it.MemberUserName == Username && it.Project_id == project_id);
            var member = beMember.FirstOrDefault();
            if (member == null) return null;

            var feature_collection = database.GetCollection<FeatureModel>(featureCollection);
            var features = await feature_collection.FindAsync(it => it.Project_id == project_id);
            var featuresList = features.ToList();

            var story_collection = database.GetCollection<StoryModel>(storyCollection);
            var stories = await story_collection.FindAsync(it => it.Project_id == project_id);
            var storyList = stories.ToList();

            var task_collection = database.GetCollection<TaskModel>(taskCollection);
            var tasks = await task_collection.FindAsync(it => it.Project_id == project_id);
            var taskList = tasks.ToList();

            var response = featuresList.Select(ft =>
            {
                return new FeatureViewModel
                {
                    _id = ft._id,
                    WorkName = ft.WorkName,
                    Description = ft.Description,
                    Status = ft.Status,
                    CreateDate = ft.CreateDate,
                    StartDate = ft.StartDate,
                    CloseDate = ft.CloseDate,
                    DoneDate = ft.DoneDate,
                    AssignTo = ft.AssignTo,
                    AssignBy = ft.AssignBy,
                    Project_id = ft.Project_id,
                    StoryList = storyList.Where(st => st.Feature_id == ft._id).Select(st =>
                    {
                        return new StoryViewModel
                        {
                            _id = st._id,
                            WorkName = st.WorkName,
                            Description = st.Description,
                            Status = st.Status,
                            CreateDate = st.CreateDate,
                            StartDate = st.StartDate,
                            CloseDate = st.CloseDate,
                            DoneDate = st.DoneDate,
                            AssignTo = st.AssignTo,
                            AssignBy = st.AssignBy,
                            Project_id = st.Project_id,
                            TaskList = taskList.Where(t => t.Story_id == st._id).Select(t =>
                            {
                                return new TaskViewModel
                                {
                                    _id = st._id,
                                    WorkName = st.WorkName,
                                    Description = st.Description,
                                    Status = st.Status,
                                    CreateDate = st.CreateDate,
                                    StartDate = st.StartDate,
                                    CloseDate = st.CloseDate,
                                    DoneDate = st.DoneDate,
                                    AssignTo = st.AssignTo,
                                    AssignBy = st.AssignBy,
                                    Project_id = st.Project_id
                                };
                            })
                        };
                    }),
                };
            });

            return response;
        }

        public async Task<FeatureModel> GetFeature(string featureid)
        {
            var isDataValid = !string.IsNullOrEmpty(featureid);
            if (!isDataValid) return new FeatureModel();
            
            var feature_collection = database.GetCollection<FeatureModel>(featureCollection);
            var features = await feature_collection.FindAsync(it => it._id == featureid);
            var result = features.FirstOrDefault();
            return result;
        }

        public async Task<bool> CreateFeature(FeatureModel request)
        {
            var isDataValid = request != null
                && !string.IsNullOrEmpty(request.WorkName);
            if (!isDataValid) return false;

            var feature_collection = database.GetCollection<FeatureModel>(featureCollection);
            request._id = Guid.NewGuid().ToString();
            request.CreateDate = DateTime.Now;
            await feature_collection.InsertOneAsync(request);

            return true;
        }

        public async Task<bool> UpdateFeature(FeatureModel request, ClaimsPrincipal User)
        {
            var isDataValid = request != null
                && !string.IsNullOrEmpty(request._id)
                && !string.IsNullOrEmpty(request.WorkName)
                && User != null;
            if (!isDataValid) return false;
            
            var Username = _UserManager.GetUserName(User);
            var member_collection = database.GetCollection<MembershipModel>(membershipCollection);
            var beMember = await member_collection.FindAsync(it => it.MemberUserName == Username && it.Project_id == request.Project_id);
            var member = beMember.FirstOrDefault();
            if (member == null) return false;

            request.AssignBy = member.MemberUserName;

            var feature_collection = database.GetCollection<FeatureModel>(featureCollection);
            await feature_collection.ReplaceOneAsync(it => it._id == request._id, request);
            return true;
        }
        
        public async Task<bool> RemoveFeature(string featureid)
        {
            var isDataValid = !string.IsNullOrEmpty(featureid);
            if (!isDataValid) return false;
            
            var feature_collection = database.GetCollection<FeatureModel>(featureCollection);
            var filter = Builders<FeatureModel>.Filter.Eq(it => it._id, featureid);
            await feature_collection.DeleteOneAsync(filter);
            return true;
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

        public class WorkModel
        {
            public string _id { get; set; }

            [Required(ErrorMessage = "กรุณาใส่ชื่อ Work")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [Display(Name = "Work name")]
            public string WorkName { get; set; }

            [StringLength(300, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
            [Display(Name = "Description")]
            public string Description { get; set; }
            public string Status { get; set; }
            public DateTime CreateDate { get; set; }

            [DataType(DataType.DateTime)]
            [Display(Name = "Start Work Date")]
            public DateTime StartDate { get; set; }

            [DataType(DataType.DateTime)]
            [Display(Name = "Close Work Date")]
            public DateTime CloseDate { get; set; }
            public DateTime? DoneDate { get; set; }

            [Display(Name = "Assign To")]
            public string AssignTo { get; set; }

            [Display(Name = "Assign By")]
            public string AssignBy { get; set; }

            [Display(Name = "Create By")]
            public string CreateBy { get; set; }
            public string Project_id { get; set; }
        }

        public class FeatureModel : WorkModel
        {
        }

        public class StoryModel : WorkModel
        {
            public string Feature_id { get; set; }
        }

        public class TaskModel : WorkModel
        {
            public string Story_id { get; set; }
        }

        public enum WorkStatus
        {
            New,
            Active,
            Done
        }
    }
}
