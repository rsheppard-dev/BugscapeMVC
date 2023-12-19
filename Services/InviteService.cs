using BugscapeMVC.Data;
using BugscapeMVC.Models;
using BugscapeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugscapeMVC.Services
{
    public class InviteService : IInviteService
    {
        private readonly ApplicationDbContext _context;
        public InviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            Invite? invite = await _context.Invites.FirstOrDefaultAsync(invite => invite.CompanyToken == token);

            if (invite is null) return false;

            try
            {
                invite.IsValid = false;
                invite.InviteeId = userId;
                invite.JoinDate = DateTimeOffset.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.Invites.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                bool result = await _context.Invites
                    .Where(invite => invite.CompanyId == companyId)
                    .AnyAsync(invite => invite.CompanyToken == token && invite.InviteeEmail == email);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites
                    .Where(invite => invite.CompanyId == companyId)
                    .Include(invite => invite.Company)
                    .Include(invite => invite.Invitor)
                    .Include(invite => invite.Invitee)
                    .Include(invite => invite.Project)
                    .FirstOrDefaultAsync(invite => invite.Id == inviteId);

                return invite;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites
                    .Where(invite => invite.CompanyId == companyId)
                    .Include(invite => invite.Company)
                    .Include(invite => invite.Invitor)
                    .FirstOrDefaultAsync(invite => invite.CompanyToken == token && invite.InviteeEmail == email);

                return invite;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateInviteAsync(Invite invite)
        {
            try
            {
                _context.Update(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            bool result = false;

            if (token is null) return result;

            try
            {
                Invite? invite = await _context.Invites.FirstOrDefaultAsync(invite => invite.CompanyToken == token);

                if (invite is null) return result;

                DateTime inviteDate = invite.InviteDate.DateTime;

                // custom validation of invite based on the date it was issued
                // in this case we are allowing an invite to be valid for 7 days
                bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                if (validDate)
                {
                    result = true;
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}