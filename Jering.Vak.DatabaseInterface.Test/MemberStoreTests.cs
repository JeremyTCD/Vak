using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jering.ASP.NET.Identity.Dapper;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using Dapper;
using System.Data;

namespace Jering.ASP.NET.Identity.Dapper.Tests
{
    [TestClass()]
    public class MemberStoreTests
    {
        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
            {
                MemberStore memberStore = new MemberStore(sqlConnection);

                Member member = new Member
                {
                    PasswordHash = "passwordHash2",
                    SecurityStamp = "securityStamp2",
                    Email = "email2",
                    NormalizedEmail = "email2"
                };

                CancellationToken concellationToken = new CancellationToken();

                await memberStore.CreateAsync(member, concellationToken);

                member = await memberStore.FindByIdAsync(member.MemberId.ToString(), concellationToken);

                Assert.AreEqual(null, member.Username);
                Assert.AreEqual("passwordHash2", member.PasswordHash);
                Assert.AreEqual("securityStamp2", member.SecurityStamp);
                Assert.AreEqual("email2", member.Email);
                Assert.AreEqual(0, member.AccessFailedCount);
                Assert.AreEqual(false, member.EmailConfirmed);
                Assert.AreEqual(false, member.LockoutEnabled);
                Assert.AreEqual(true, member.TwoFactorEnabled);
                Assert.AreEqual(null, member.LockoutEndDateUtc);
                Assert.AreEqual(null, member.NormalizedUsername);
                Assert.AreEqual("email2", member.NormalizedEmail);

                await memberStore.DeleteAsync(member, concellationToken);

                await sqlConnection.ExecuteAsync("DBCC CHECKIDENT('[dbo].[Members]', RESEED, 1);", commandType: CommandType.Text);
            }
        }

        //[TestMethod()]
        //public void DeleteAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void FindByIdAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void FindByNameAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void GetNormalizedUserNameAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void GetUserIdAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void GetUserNameAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void SetNormalizedUserNameAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void SetUserNameAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void UpdateAsyncTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DisposeTest()
        //{
        //    Assert.Fail();
        //}
    }
}