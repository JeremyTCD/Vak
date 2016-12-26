using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.RequestModels.DynamicForm
{
    public class GetDynamicFormRequestModel
    {
        public string requestModelName { get; set; }
        public bool getAfTokens { get; set; }
    }
}
