using Microsoft.EntityFrameworkCore;
using SocialNetwork.DLL.Entities;
using SocialNetwork.DLL.Repositories.Base;

namespace SocialNetwork.DLL.Repositories;

public class MessageRepository : Repository<MessageEntity>
{
    public MessageRepository(ApplicationDbContext db)
        : base(db)
    {

    }

    public async Task<List<MessageEntity>> GetMessages(UserEntity sender, UserEntity recipient)
    {
        Set.Include(x => x.Recipient);
        Set.Include(x => x.Sender);

        var from = await Set.AsQueryable().Where(x => x.SenderId == sender.Id && x.RecipientId == recipient.Id).ToListAsync();
        var to = await Set.AsQueryable().Where(x => x.SenderId == recipient.Id && x.RecipientId == sender.Id).ToListAsync();

        return from.Concat(to).OrderBy(x => x.Id).ToList();
    }
}