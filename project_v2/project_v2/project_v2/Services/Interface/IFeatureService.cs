using project_v2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_v2.Services.Interface
{
    public interface IFeatureService
    {
        void CreateFeature(FeatureModel model);
        List<FeatureModel> GetFeatures(string projectId);
        FeatureModel GetFeature(string featureId);
        void EditFeature(string featureId, FeatureModel model);
        void DeleteFeature(string featureId);
    }
}
