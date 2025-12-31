using APIService;
using APIService.Holiday;
using BaiduMapService.API;
using BaiduMapService.Model;
using BaiduMapService.Service;
using CommonHelper;
using CommonHelper.Model;
using LogService.Service;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThinkAPIService.API;
using ThinkAPIService.Service;

namespace TestProgramma
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (BaseCommand cmd = new BaseCommand())
            {
                cmd.Initialize("");

                //cmd.OrganizationServiceAdmin
                QueryExpression qe = new QueryExpression("solution");
                qe.TopCount = 1;
                qe.Criteria.AddCondition("uniquename", ConditionOperator.Equal, "dev_rektec_onsite_1208_my");
                var ec = cmd.OrganizationServiceAdmin.RetrieveMultiple(qe);

                Entity solution = ec != null && ec.Entities.Any() ? ec.Entities.FirstOrDefault() : null;
                
            }
        }
    }
}
