namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        public bool IsAutoTargetTrackable
        {
            get
            {
                ResolveReferences();

                return
                    itemInventory == null ||
                    itemInventory.IsAutoTargetTrackable;
            }
        }

        public bool IsInvisibleByCloak
        {
            get
            {
                ResolveReferences();

                return
                    itemInventory != null &&
                    itemInventory.IsInvisibilityCloakActive;
            }
        }
    }
}
