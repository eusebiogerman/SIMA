using SIMA.Domain.Models.Intefaces;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;

namespace SIMA.Domain.Models.Params
{
    public class UserParam : IParam
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserPassword { get; set; }
        public string? CountryCode { get; set; }
        public int? offset { get; set; } = 0;
        public int? limit { get; set; } = 10;

        public UserParam()
        {
        }
        public UserParam(IPaging page)
        {
            offset = page.Offset;
            limit = page.Limit;
        }

        public void SetPage(IPaging page)
        {
            offset = page.Offset;
            limit = page.Limit;
        }
        public void ResetParam(int? inoffset = 0, int? inlimit = 10)
        {
            throw new NotImplementedException();
        }
    }
}