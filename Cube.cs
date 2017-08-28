using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KIC
{
    /// <summary>
    /// A large cube that spawns up to 6 Bytes at a time.  Aside from spawning Bytes, stays completely still.
    /// Kic is able to stand on top of and touch a Cube without taking damage.
    /// If any Bytes are destroyed or lost, the Cube will generate new Bytes to replace them.
    /// </summary>
    class Cube : Enemy
    {
        private int Count = 0;
        private const int MAX_BYTES = 6; // max number of Bytes allowed to spawn
        private const int MAX_COUNT = 150; // how many frames between spawning each Byte
        public int TileNum; // keeps track of the tile index within the list
        private int Bytes = 0; // keeps track of how many Bytes have been spawned

        public Cube() : base()
        {
            Actions.Add(new Action(1, Texture, new Rectangle(0, 24, 32, 32), new Vector2(16, 16), 0, new Rectangle(-16, -16, 32, 32), false, false, 0)); // idle
            Actions.Add(new Action(3, Texture, new Rectangle(33, 24, 32, 32), new Vector2(16, 16), 6, new Rectangle(-16, -16, 32, 32), true, false, 1)); // spawning a Byte
            ID = 1;
            Directional = false; // does not face different directions
            stats.SetStats(20, 0, 10); // HP - 20, Attack - 0, Defense - 10
            cAction = Actions[0];
            Platform = true; // treat this enemy like any other solid floor that Kic can stand on
            EnergyDrop = new int[3] { 10, 15, 20 };
        }

        public override void SetTileNum(int Num) // sets the Cube as another floor tile in a room
        {
            TileNum = Num;
            HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);
        }

        private void ChildKilled(object sender, EventArgs e)
        {
            Bytes--;
        }

        public override void Update(Room room, KIC_Object Kic)
        {
            if (stats.HP > 0) // if alive
            {
                if (Bytes < MAX_BYTES)
                {
                    if (Count > MAX_COUNT)
                    {
                        cAction = Actions[1];
                        Count = 0;
                    }

                    Count++;
                }

                if (cAction.Num == 1) // spawning a new Byte
                {
                    if (!animation.AnimateNL(cAction)) // when done spawning
                    {
                        room.Enemies.Add(room.E_Spawner.Spawn(2, new Vector2(Pos.X, Pos.Y - cAction.Center.Y - 7), Texture));
                        if (Pos.X > Kic.Pos.X) // spawn in the direction of Kic
                            room.Enemies[room.Enemies.Count - 1].SetByte(false);
                        else
                            room.Enemies[room.Enemies.Count - 1].SetByte(true);

                        room.Enemies[room.Enemies.Count - 1].Killed += ChildKilled; // set the event for when a child dies
                        Bytes++;

                        cAction = Actions[0];  // go back to idle
                    }
                }
            }

            if (stats.HP <= 0 ) // if destroyed
            {
                Alive = false;
                room.EnemyRecs[TileNum] = new Rectangle((int)room.UpperLeft.X, (int)room.UpperLeft.Y, 0, 0); // make this Cube non-collidable
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (stats.HP > 0) // if alive
                spriteBatch.Draw(Texture, new Vector2((int)Pos.X, (int)Pos.Y), cAction.Rec[cAction.CurrentFrame], Color.White, 0, cAction.Center, 1, SpriteEffects.None, 0);
        }
    }
}
