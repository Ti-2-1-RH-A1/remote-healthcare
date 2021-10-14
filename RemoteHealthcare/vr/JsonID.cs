namespace RemoteHealthcare.VR
{
    internal class JsonID
    {
        // Scene ID's
        public static readonly string SCENE_GET = "scene/get";
        public static readonly string SCENE_RESET = "scene/reset";
        public static readonly string SCENE_SAVE = "scene/save";
        public static readonly string SCENE_LOAD = "scene/load";
        public static readonly string SCENE_RAYCAST = "scene/raycast";

        // Scene/Node ID's
        public static readonly string SCENE_NODE_ADD = "scene/node/add";
        public static readonly string SCENE_NODE_MOVETO = "scene/node/moveto";
        public static readonly string SCENE_NODE_UPDATE = "scene/node/update";
        public static readonly string SCENE_NODE_DELETE = "scene/node/delete";
        public static readonly string SCENE_NODE_ADDLAYER = "scene/node/addlayer";
        public static readonly string SCENE_NODE_DELLAYER = "scene/node/dellayer";
        public static readonly string SCENE_NODE_FIND = "scene/node/find";

        // Scene/Terrain ID's
        public static readonly string SCENE_TERRAIN_ADD = "scene/terrain/add";
        public static readonly string SCENE_TERRAIN_UPDATE = "scene/terrain/update";
        public static readonly string SCENE_TERRAIN_DELETE = "scene/terrain/delete";
        public static readonly string SCENE_TERRAIN_GETHEIGHT = "scene/terrain/getheight";

        // Scene/Panel ID's
        public static readonly string SCENE_PANEL_CLEAR = "scene/panel/clear";
        public static readonly string SCENE_PANEL_DRAWLINES = "scene/panel/drawlines";
        public static readonly string SCENE_PANEL_DRAWTEXT = "scene/panel/drawtext";
        public static readonly string SCENE_PANEL_IMAGE = "scene/panel/image";
        public static readonly string SCENE_PANEL_SETCLEARCOLOR = "scene/panel/setclearcolor";
        public static readonly string SCENE_PANEL_SWAP = "scene/panel/swap";

        // Scene/Skybox ID's
        public static readonly string SCENE_SKYBOX_SETTIME = "scene/skybox/settime";
        public static readonly string SCENE_SKYBOX_UPDATE = "scene/skybox/update";

        // Scene/Road ID's
        public static readonly string SCENE_ROAD_ADD = "scene/road/add";
        public static readonly string SCENE_ROAD_UPDATE = "scene/road/update";

        // Route ID's
        public static readonly string ROUTE_ADD = "route/add";
        public static readonly string ROUTE_UPDATE = "route/update";
        public static readonly string ROUTE_DELETE = "route/delete";
        public static readonly string ROUTE_FOLLOW = "route/follow";
        public static readonly string ROUTE_FOLLOW_SPEED = "route/follow/speed";
        public static readonly string ROUTE_SHOW = "route/show";

        // Misc ID's
        public static readonly string GET = "get";
        public static readonly string SETCALLBACK = "setcallback";
        public static readonly string PLAY = "play";
        public static readonly string PAUSE = "pause";
    }
}