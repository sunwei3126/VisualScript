namespace IoTLogic.Core.Collections
{
    public interface INotifiedCollectionItem
    {
        void BeforeAdd();
        void AfterAdd();
        void BeforeRemove();
        void AfterRemove();
    }
}
