namespace Gribble.Model
{
    public class Query
    {
        public enum OperationType { Query, CopyTo, SyncWith }

        public OperationType Operation = OperationType.Query;

        public Select Select;
        public Sync SyncWith;
        public Insert CopyTo; 
    }
}