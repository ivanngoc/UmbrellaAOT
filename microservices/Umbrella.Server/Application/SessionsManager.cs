using System.Collections.Concurrent;

namespace Umbrella.Server.Application
{
    public class SessionsManager
    {
        private readonly ConcurrentDictionary<Guid, Session> sessions = new ConcurrentDictionary<Guid, Session>();
        public IEnumerable<Session> Sessions => sessions.Values;

        public void Add(Session session)
        {
            sessions.TryAdd(session.Id, session);
        }

        public void Remove(Session session)
        {
            throw new System.NotImplementedException();
        }
    }
}
