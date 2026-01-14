using BussinessObjects.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace BE.Odata
{
    public static class ODataEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<User>("Users");
            builder.EntitySet<Item>("Items");
            builder.EntitySet<GameNews>("GameNews");

            return builder.GetEdmModel();
        }
    }
}
