using System;
using Microsoft.Xna.Framework;

namespace KIC
{
    /// <summary>
    /// This class handles any reading of XML files.
    /// Makes calls to the XML class, which handles all XML actions such as reading a node, or a list of nodes in a file.
    /// </summary>
    class Loader
    {
        /// <summary>
        /// Reads a room file and returns the room as an object.
        /// In case of file corruption, default values are used to prevent (most) major issues.
        /// </summary>
        /// <param name="Num">The number (ID) of the room being loaded</param>
        /// <returns></returns>
        public Room LoadRoom(int Num)
        {
            Room room = new Room();
            int parsedint;
            float parsedflt;
            float temp;

            XML.OpenDocument("room" + Num + ".xml");

            // StartingPoint
            string info = XML.ReadInnerTextFrom("XnaContent/Asset/StartingPoint");
            string[] split = info.Split(' ');
            parsedflt = -9999;
            float.TryParse(split[0], out parsedflt);
            room.StartingPoint.X = parsedflt;
            parsedflt = -9999;
            float.TryParse(split[1], out parsedflt);
            room.StartingPoint.Y = parsedflt;

            // Blocks
            XML.CreateNodeListOf("XnaContent/Asset/Blocks/Item");
            int count = XML.NodeListCount();
            // create new instances of the blocks
            for (int i = 0; i < count; i++)
            {
                room.Blocks.Add(new Block());
            }
            // get the info for each block
            for (int i = 0; i < count; i++)
            {
                // Pos
                info = XML.ReadInnerTextFrom(i, "Pos");
                split = info.Split(' ');
                parsedflt = 0;
                float.TryParse(split[0], out parsedflt);
                room.Blocks[i].Pos.X = parsedflt;
                parsedflt = 0;
                float.TryParse(split[1], out parsedflt);
                room.Blocks[i].Pos.Y = parsedflt;

                // RoomID
                info = XML.ReadInnerTextFrom(i, "RoomID");
                parsedint = 0;
                int.TryParse(info, out parsedint);
                room.Blocks[i].RoomID = parsedint;

                // Doors
                XML.SaveThenMakeActive("Blocks", i, "Doors/Item"); // save blocks nodelist
                int count2 = XML.NodeListCount();
                // create doors
                for (int j = 0; j < count2; j++)
                {
                    room.Blocks[i].Doors.Add(new Door());
                }
                // load door info
                for (int j = 0; j < count2; j++)
                {
                    // Area
                    info = XML.ReadInnerTextFrom(j, "Area");
                    split = info.Split(' ');
                    parsedint = 0;
                    int.TryParse(split[0], out parsedint);
                    room.Blocks[i].Doors[j].Area.X = parsedint;
                    parsedint = 0;
                    int.TryParse(split[1], out parsedint);
                    room.Blocks[i].Doors[j].Area.Y = parsedint;
                    parsedint = 0;
                    int.TryParse(split[2], out parsedint);
                    room.Blocks[i].Doors[j].Area.Width = parsedint;
                    parsedint = 0;
                    int.TryParse(split[3], out parsedint);
                    room.Blocks[i].Doors[j].Area.Height = parsedint;

                    // BlockDes
                    info = XML.ReadInnerTextFrom(j, "BlockDes");
                    parsedint = 0;
                    int.TryParse(info, out parsedint);
                    room.Blocks[i].Doors[j].BlockDes = parsedint;

                    // RoomDes
                    info = XML.ReadInnerTextFrom(j, "RoomDes");
                    parsedint = 0;
                    int.TryParse(info, out parsedint);
                    room.Blocks[i].Doors[j].RoomDes = parsedint;

                    // Side
                    info = XML.ReadInnerTextFrom(j, "Side");
                    parsedint = 0;
                    int.TryParse(info, out parsedint);
                    room.Blocks[i].Doors[j].Side = parsedint;
                }

                //restore blocks nodelist
                XML.RestoreOldNodeList("Blocks");
            }

            //Tiles
            XML.CreateNodeListOf("XnaContent/Asset/Tiles/Item");
            count = XML.NodeListCount();

            // create new instances of the tiles
            for (int i = 0; i < count; i++)
            {
                room.Tiles.Add(new Tile());
            }

            // get info for tiles
            for (int i = 0; i < count; i++)
            {
                // Pos
                info = XML.ReadInnerTextFrom(i, "Pos");
                split = info.Split(' ');
                parsedflt = 0;
                float.TryParse(split[0], out parsedflt);
                room.Tiles[i].Pos.X = parsedflt;
                parsedflt = 0;
                float.TryParse(split[1], out parsedflt);
                room.Tiles[i].Pos.Y = parsedflt;

                // Type
                info = XML.ReadInnerTextFrom(i, "Type");
                parsedint = 0;
                int.TryParse(info, out parsedint);
                room.Tiles[i].Type = parsedint;

                // Selection
                info = XML.ReadInnerTextFrom(i, "Selection");
                parsedint = 0;
                int.TryParse(info, out parsedint);
                room.Tiles[i].Selection = parsedint;

                // Rec
                info = XML.ReadInnerTextFrom(i, "Rec");
                split = info.Split(' ');
                parsedint = 0;
                int.TryParse(split[0], out parsedint);
                room.Tiles[i].Rec.X = parsedint;
                parsedint = 0;
                int.TryParse(split[1], out parsedint);
                room.Tiles[i].Rec.Y = parsedint;
                parsedint = 0;
                int.TryParse(split[2], out parsedint);
                room.Tiles[i].Rec.Width = parsedint;
                parsedint = 0;
                int.TryParse(split[3], out parsedint);
                room.Tiles[i].Rec.Height = parsedint;

                // Source
                info = XML.ReadInnerTextFrom(i, "Source");
                split = info.Split(' ');
                parsedint = 0;
                int.TryParse(split[0], out parsedint);
                room.Tiles[i].Source.X = parsedint;
                parsedint = 0;
                int.TryParse(split[1], out parsedint);
                room.Tiles[i].Source.Y = parsedint;
                parsedint = 0;
                int.TryParse(split[2], out parsedint);
                room.Tiles[i].Source.Width = parsedint;
                parsedint = 0;
                int.TryParse(split[3], out parsedint);
                room.Tiles[i].Source.Height = parsedint;

                // Rotation
                info = XML.ReadInnerTextFrom(i, "Rotation");
                parsedflt = 0;
                float.TryParse(info, out parsedflt);
                room.Tiles[i].Rotation = parsedflt;
            }

            // TexName
            info = XML.ReadInnerTextFrom("XnaContent/Asset/TexName");
            room.TexName = info;

            // UpperLeft
            info = XML.ReadInnerTextFrom("XnaContent/Asset/UpperLeft");
            split = info.Split(' ');
            parsedflt = 0;
            float.TryParse(split[0], out parsedflt);
            room.UpperLeft.X = parsedflt;
            parsedflt = 0;
            float.TryParse(split[1], out parsedflt);
            room.UpperLeft.Y = parsedflt;

            // LowerRight
            info = XML.ReadInnerTextFrom("XnaContent/Asset/LowerRight");
            split = info.Split(' ');
            parsedflt = 0;
            float.TryParse(split[0], out parsedflt);
            room.LowerRight.X = parsedflt;
            parsedflt = 0;
            float.TryParse(split[1], out parsedflt);
            room.LowerRight.Y = parsedflt;

            // MapPos
            info = XML.ReadInnerTextFrom("XnaContent/Asset/MapPos");
            split = info.Split(' ');
            parsedint = 0;
            int.TryParse(split[0], out parsedint);
            room.MapPos.X = parsedint;
            parsedint = 0;
            int.TryParse(split[1], out parsedint);
            room.MapPos.Y = parsedint;

            // ID
            info = XML.ReadInnerTextFrom("XnaContent/Asset/ID");
            parsedint = 0;
            int.TryParse(info, out parsedint);
            room.ID = parsedint;

            //EnemyList
            if (XML.SelectSingleNode("XnaContent/Asset/EnemyList") != null)
            {
                info = XML.ReadInnerTextFrom("XnaContent/Asset/EnemyList");
                split = info.Split(' ');
                if (split[0] != "") // if EnemyList isn't empty
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        parsedint = -1;
                        int.TryParse(split[i], out parsedint);
                        room.EnemyList.Add(parsedint);
                    }
                }
            }

            //EnemyPos
            if (XML.SelectSingleNode("XnaContent/Asset/EnemyPos") != null)
            {
                info = XML.ReadInnerTextFrom("XnaContent/Asset/EnemyPos");
                split = info.Split(' ');
                if (split[0] != "") // if EnemyPos isn't empty
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        parsedflt = 0;
                        temp = 0;
                        float.TryParse(split[i], out parsedflt);
                        float.TryParse(split[i + 1], out temp);
                        room.EnemyPos.Add(new Vector2(parsedflt, temp));
                        i++; // skip every other element, since they're in pairs
                    }
                }
            }

            XML.CloseDocument();

            room.CreatePlatformRecs(); // this takes all the tiles and lumps any adjacent platforming rectangles into one larger rectangle (for more effecient platform checks)

            return room;
        }

        /// <summary>
        /// Loads the settings set in the options screen.
        /// Also handles default values if there aren't any set yet.
        /// </summary>
        public void LoadSettings()
        {
            int parsedint;
            bool parsedbool;

            XML.OpenDocument("settings.xml");

            string info = XML.ReadInnerTextFrom("XnaContent/FullScreen");
            parsedbool = false;
            bool.TryParse(info, out parsedbool);
            Settings.FullScreen = parsedbool;

            info = XML.ReadInnerTextFrom("XnaContent/AutoPause");
            parsedbool = true;
            bool.TryParse(info, out parsedbool);
            Settings.AutoPause = parsedbool;

            info = XML.ReadInnerTextFrom("XnaContent/MasterVolume");
            parsedint = 100;
            int.TryParse(info, out parsedint);
            if (parsedint < 0)
                parsedint = 0;
            if (parsedint > 100)
                parsedint = 100;
            Settings.MasterVolume = parsedint;

            info = XML.ReadInnerTextFrom("XnaContent/SoundVolume");
            parsedint = 100;
            int.TryParse(info, out parsedint);
            if (parsedint < 0)
                parsedint = 0;
            if (parsedint > 100)
                parsedint = 100;
            Settings.SoundVolume = parsedint;

            info = XML.ReadInnerTextFrom("XnaContent/MusicVolume");
            parsedint = 100;
            int.TryParse(info, out parsedint);
            if (parsedint < 0)
                parsedint = 0;
            if (parsedint > 100)
                parsedint = 100;
            Settings.MusicVolume = parsedint;

            XML.CloseDocument();
        }
    }
}
