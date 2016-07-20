//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Jering.ASP.NET.Identity.Dapper;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Configuration;
//using System.Data;
//using System.Diagnostics;
//using System.Data.SqlClient;
//using Dapper;

//namespace Jering.AspNet.Identity.Dapper.Tests
//{
//    [TestClass()]
//    public class MemberTableTests
//    {
//        [TestMethod()]
//        public async Task GetMemberUsernameTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Assert.AreEqual("username1", await memberTable.GetMemberUsernameAsync(1));
//            }
//        }

//        [TestMethod()]
//        public async Task GetMemberIdTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Assert.AreEqual(1, await memberTable.GetMemberIdByUsernameAsync("username1"));
//            }
//        }

//        [TestMethod()]
//        public async Task GetMemberByIdTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Member member = await memberTable.GetMemberAsync(1);

//                Assert.AreEqual(1, member.MemberId);
//                Assert.AreEqual("username1", member.Username);
//                Assert.AreEqual("passwordHash1", member.PasswordHash);
//                Assert.AreEqual("securityStamp1", member.SecurityStamp);
//                Assert.AreEqual("email1", member.Email);
//                Assert.AreEqual(0, member.AccessFailedCount);
//                Assert.AreEqual(true, member.EmailConfirmed);
//                Assert.AreEqual(true, member.LockoutEnabled);
//                Assert.AreEqual(true, member.TwoFactorEnabled);
//                Assert.AreEqual(null, member.LockoutEndDateUtc);
//            }
//        }

//        [TestMethod()]
//        public async Task GetMemberByMemberNameTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Member member = await memberTable.GetMemberByUsernameAsync("username1");

//                Assert.AreEqual(member.MemberId, 1);
//                Assert.AreEqual(member.Username, "username1");
//                Assert.AreEqual(member.PasswordHash, "passwordHash1");
//                Assert.AreEqual(member.SecurityStamp, "securityStamp1");
//                Assert.AreEqual(member.Email, "email1");
//                Assert.AreEqual(member.AccessFailedCount, 0);
//                Assert.AreEqual(member.EmailConfirmed, true);
//                Assert.AreEqual(member.LockoutEnabled, true);
//                Assert.AreEqual(member.TwoFactorEnabled, true);
//                Assert.AreEqual(member.LockoutEndDateUtc, null);
//            }
//        }

//        [TestMethod()]
//        public async Task GetMemberByEmailTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Member member = await memberTable.GetMemberByEmailAsync("email1");

//                Assert.AreEqual(member.MemberId, 1);
//                Assert.AreEqual(member.Username, "username1");
//                Assert.AreEqual(member.PasswordHash, "passwordHash1");
//                Assert.AreEqual(member.SecurityStamp, "securityStamp1");
//                Assert.AreEqual(member.Email, "email1");
//                Assert.AreEqual(member.AccessFailedCount, 0);
//                Assert.AreEqual(member.EmailConfirmed, true);
//                Assert.AreEqual(member.LockoutEnabled, true);
//                Assert.AreEqual(member.TwoFactorEnabled, true);
//                Assert.AreEqual(member.LockoutEndDateUtc, null);
//            }
//        }

//        [TestMethod()]
//        public async Task GetPasswordHashTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Assert.AreEqual(await memberTable.GetMemberPasswordHashAsync(1), "passwordHash1");
//            }
//        }

//        [TestMethod()]
//        public async Task SetPasswordHashTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                await memberTable.SetMemberPasswordHashAsync(1, "passwordHash2");

//                Assert.AreEqual("passwordHash2", await memberTable.GetMemberPasswordHashAsync(1));

//                await memberTable.SetMemberPasswordHashAsync(1, "passwordHash1");
//            }
//        }

//        [TestMethod()]
//        public async Task GetSecurityStampTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Assert.AreEqual(await memberTable.GetMemberSecurityStampAsync(1), "securityStamp1");
//            }
//        }

//        [TestMethod()]
//        public async Task InsertMemberTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                Member member = new Member
//                {
//                    Username = "username2",
//                    PasswordHash = "passwordHash2",
//                    SecurityStamp = "securityStamp2",
//                    Email = "email2",
//                    EmailConfirmed = true,
//                    TwoFactorEnabled = true,
//                    LockoutEnabled = true
//                };

//                await memberTable.InsertMemberAsync(member);

//                member = await memberTable.GetMemberAsync(2);

//                Assert.AreEqual(member.Username, "username2");
//                Assert.AreEqual(member.PasswordHash, "passwordHash2");
//                Assert.AreEqual(member.SecurityStamp, "securityStamp2");
//                Assert.AreEqual(member.Email, "email2");
//                Assert.AreEqual(member.AccessFailedCount, 0);
//                Assert.AreEqual(member.EmailConfirmed, true);
//                Assert.AreEqual(member.LockoutEnabled, true);
//                Assert.AreEqual(member.TwoFactorEnabled, true);
//                Assert.AreEqual(member.LockoutEndDateUtc, null);

//                await memberTable.DeleteMemberAsync(2);

//                await sqlConnection.ExecuteAsync("DBCC CHECKIDENT('[dbo].[Members]', RESEED, 1);", commandType: CommandType.Text);
//            }
//        }

//        [TestMethod()]
//        public async Task DeleteMemberTest()
//        {
//            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                MemberTable<Member> memberTable = new MemberTable<Member>(sqlConnection);

//                await memberTable.DeleteMemberAsync(1);

//                int numRows = await sqlConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[Members];", commandType: CommandType.Text);

//                Assert.AreEqual(numRows, 0);

//                await sqlConnection.ExecuteAsync("DBCC CHECKIDENT('[dbo].[Members]', RESEED, 0);", commandType: CommandType.Text);

//                await memberTable.InsertMemberAsync(
//                  new Member
//                  {
//                      Username = "username1",
//                      PasswordHash = "passwordHash1",
//                      SecurityStamp = "securityStamp1",
//                      Email = "email1",
//                      EmailConfirmed = true,
//                      TwoFactorEnabled = true,
//                      LockoutEnabled = true
//                  });
//            }
//        }

//        [TestMethod()]
//        public async Task UpdateTest()
//        {
//            using (SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString))
//            {
//                DateTime now = DateTime.Now;

//                MemberTable<Member> memberTable = new MemberTable<Member>(SqlConnection);

//                await memberTable.UpdateMemberAsync(new Member
//                {
//                    MemberId = 1,
//                    Username = "username2",
//                    PasswordHash = "passwordHash2",
//                    SecurityStamp = "securityStamp2",
//                    Email = "email2",
//                    EmailConfirmed = false,
//                    TwoFactorEnabled = false,
//                    LockoutEnabled = false,
//                    AccessFailedCount = 1,
//                    LockoutEndDateUtc = now
//                });

//                Member member = await memberTable.GetMemberAsync(1);

//                Assert.AreEqual(member.Username, "username2");
//                Assert.AreEqual(member.PasswordHash, "passwordHash2");
//                Assert.AreEqual(member.SecurityStamp, "securityStamp2");
//                Assert.AreEqual(member.Email, "email2");
//                Assert.AreEqual(member.AccessFailedCount, 1);
//                Assert.AreEqual(member.EmailConfirmed, false);
//                Assert.AreEqual(member.LockoutEnabled, false);
//                Assert.AreEqual(member.TwoFactorEnabled, false);
//                Assert.AreEqual(member.LockoutEndDateUtc.ToString(), now.ToString());

//                await memberTable.UpdateMemberAsync(new Member
//                {
//                    Username = "username1",
//                    PasswordHash = "passwordHash1",
//                    SecurityStamp = "securityStamp1",
//                    Email = "email1",
//                    EmailConfirmed = true,
//                    TwoFactorEnabled = true,
//                    LockoutEnabled = true,
//                    AccessFailedCount = 0,
//                    LockoutEndDateUtc = null
//                });
//            }
//        }
//    }
//}