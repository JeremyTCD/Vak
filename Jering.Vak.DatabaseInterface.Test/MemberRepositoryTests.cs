using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Data;
using Dapper;

namespace Jering.Vak.DatabaseInterface.Test
{
    [Collection("DatabaseCollection")]
    public class MemberRepositoryTests
    {
        private DatabaseFixture _databaseFixture { get; }
        private SqlConnection _sqlConnection { get; }
        private RoleRepository _roleRepository { get; }
        private ClaimRepository _claimRepository { get; }
        private MemberRepository _memberRepository { get; }
        private Func<Task> _resetClaimsTable { get; }
        private Func<Task> _resetRolesTable { get; }
        private Func<Task> _resetMembersTable { get; }

        public MemberRepositoryTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _sqlConnection = _databaseFixture.SqlConnection;
            _roleRepository = databaseFixture.RoleRepository;
            _claimRepository = databaseFixture.ClaimRepository;
            _memberRepository = databaseFixture.MemberRepository;
            _resetClaimsTable = _databaseFixture.ResetClaimsTable;
            _resetRolesTable = _databaseFixture.ResetRolesTable;
            _resetMembersTable = _databaseFixture.ResetMembersTable;
        }

        [Fact]
        public async Task CreateMemberAsyncTest()
        {
            // Arrange
            await _resetMembersTable();

            // Act
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");

            // Assert
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.Unicode.GetBytes("$PassworD" + "Email@Jering.com"));
            Assert.Equal(1, member.MemberId);
            Assert.Equal("Email@Jering.com", member.Email);
            Assert.Equal(hash, member.PasswordHash);
            Assert.Equal(null, member.Username);
            Assert.Equal(null, member.SecurityStamp);
            Assert.Equal(false, member.EmailConfirmed);
            Assert.Equal(false, member.TwoFactorEnabled);
        }

        [Fact]
        public async Task CreateMemberAsync_UniqueEmailTest()
        {
            // Arrange
            await _resetMembersTable();

            // Act
            await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");
            SqlException sqlException = await Assert.
              ThrowsAsync<SqlException>(async () => await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD"));

            // Assert
            Assert.Equal(51000, sqlException.Number);
            Assert.Equal("An account with this email already exists.", sqlException.Message);
        }

        [Fact]
        public async Task DeleteMemberAsyncTest()
        {
            // TODO: ensure that deletion cascades

            // Arrange
            await _resetMembersTable();
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");

            // Act
            await _memberRepository.DeleteMemberAsync(member.MemberId);

            // Assert
            Assert.Equal(0, await _sqlConnection.ExecuteScalarAsync<int>("Select Count(*) from [dbo].[Members]", commandType: CommandType.Text));
        }

        [Fact]
        public async Task AddMemberRoleAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetMembersTable();
            Role role = await _roleRepository.CreateRoleAsync("Name1");
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");

            // Act
            await _memberRepository.AddMemberRoleAsync(member.MemberId, role.RoleId);

            // Assert
            Role retrievedRole = (await _memberRepository.GetMemberRolesAsync(member.MemberId)).First();
            Assert.Equal(1, retrievedRole.RoleId);
            Assert.Equal("Name1", retrievedRole.Name);
        }

        [Fact]
        public async Task DeleteMemberRoleAsyncTest()
        {
            // Arrange
            await _resetRolesTable();
            await _resetMembersTable();
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");
            Role role = await _roleRepository.CreateRoleAsync("Name1");
            await _memberRepository.AddMemberRoleAsync(member.MemberId, role.RoleId);

            // Act
            await _memberRepository.DeleteMemberRoleAsync(member.MemberId, role.RoleId);

            // Assert
            List<Role> roles = (await _memberRepository.GetMemberRolesAsync(role.RoleId)).ToList<Role>();
            Assert.Equal(0, roles.Count);
        }

        [Fact]
        public async Task GetMemberRolesAsyncTest()
        {
            // Arrange
            await _resetMembersTable();
            await _resetRolesTable();
            Role role1 = await _roleRepository.CreateRoleAsync("Name1");
            Role role2 = await _roleRepository.CreateRoleAsync("Name2");
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");
            await _memberRepository.AddMemberRoleAsync(member.MemberId, role1.RoleId);
            await _memberRepository.AddMemberRoleAsync(member.MemberId, role2.RoleId);

            // Act
            List<Role> roles = (await _memberRepository.GetMemberRolesAsync(member.MemberId)).ToList<Role>();

            // Assert
            Assert.Equal("Name1", roles[0].Name);
            Assert.Equal("Name2", roles[1].Name);
        }

        [Fact]
        public async Task AddMemberClaimAsyncTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetMembersTable();
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");

            // Act
            await _memberRepository.AddMemberClaimAsync(member.MemberId, claim.ClaimId);

            // Assert
            Claim retrievedClaim = (await _memberRepository.GetMemberClaimsAsync(member.MemberId)).First();
            Assert.Equal(1, retrievedClaim.ClaimId);
            Assert.Equal("Type1", retrievedClaim.Type);
            Assert.Equal("Value1", retrievedClaim.Value);
        }

        [Fact]
        public async Task DeleteMemberClaimAsyncTest()
        {
            // Arrange
            await _resetClaimsTable();
            await _resetMembersTable();
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");
            Claim claim = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            await _memberRepository.AddMemberClaimAsync(member.MemberId, claim.ClaimId);

            // Act
            await _memberRepository.DeleteMemberClaimAsync(member.MemberId, claim.ClaimId);

            // Assert
            List<Claim> claims = (await _memberRepository.GetMemberClaimsAsync(claim.ClaimId)).ToList<Claim>();
            Assert.Equal(0, claims.Count);
        }

        [Fact]
        public async Task GetMemberClaimsAsyncTest()
        {
            // Arrange
            await _resetMembersTable();
            await _resetClaimsTable();
            Claim claim1 = await _claimRepository.CreateClaimAsync("Type1", "Value1");
            Claim claim2 = await _claimRepository.CreateClaimAsync("Type2", "Value2");
            Member member = await _memberRepository.CreateMemberAsync("Email@Jering.com", "$PassworD");
            await _memberRepository.AddMemberClaimAsync(member.MemberId, claim1.ClaimId);
            await _memberRepository.AddMemberClaimAsync(member.MemberId, claim2.ClaimId);

            // Act
            List<Claim> claims = (await _memberRepository.GetMemberClaimsAsync(member.MemberId)).ToList<Claim>();

            // Assert
            Assert.Equal("Type1", claims[0].Type);
            Assert.Equal("Value1", claims[0].Value);
            Assert.Equal("Type2", claims[1].Type);
            Assert.Equal("Value2", claims[1].Value);
        }
    }
}
