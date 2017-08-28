using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KIC
{
    /// <summary>
    /// A basic enemy that is spawned by Cubes.  Runs along the floor, and will change directions if it hits any walls or other solid objects.
    /// If it ever comes into contact with Kic, it'll do contact damage and keep running.  Will always die in one hit.
    /// </summary>
    class Byte : Enemy
    {
        public Vector2 Velocity;
        public Boolean FacingRight;
        public Boolean Airborn;
        public const float MAX_FALL = .12f;
        public const float MAX_VEL_Y = 3;
        public const float MAX_VEL_X = 2;

        public Byte() : base()
        {
            Actions.Add(new Action(2, Texture, new Rectangle(132, 24, 13, 10), new Vector2(6, 6), 6, new Rectangle(-6, -6, 13, 10), false, false, 0)); // running (only action)
            cAction = Actions[0];
            ID = 2;
            Directional = true; // faces left or right
            stats.SetStats(1, 1, 0); // HP - 1, Attack - 1, Defense - 0
            Velocity.Y = -1;
            EnergyDrop = new int[3]{2, 0, 0};
        }

        public void Notify() // tells the parent Cube when it dies
        {
            if (Killed == null)
                return;
            Killed(this, new EventArgs());
        }


        public override void SetByte(Boolean facingright) // initializes the Byte (which direction it starts moving)
        {
            FacingRight = facingright;
            if (FacingRight)
                Velocity.X = MAX_VEL_X;
            else
                Velocity.X = -MAX_VEL_X;
        }

        public override void Update(Room room, KIC_Object Kic)
        {
            HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);

            room.CheckTileCollision(ref Velocity, HDRec, ref Pos, cAction.HDRec);

            if (Velocity.X == 0) // there is a tile collision, change direction
            {
                if (FacingRight) // right
                    FacingRight = false;

                else // left
                    FacingRight = true;
            }

            HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);

            Airborn = !room.OnSolidGround(HDRec, Velocity.Y); // if not on solid ground, set as airborn
            if (Airborn)
            {  // let gravity do its thing if airborn
                Velocity.Y += MAX_FALL;
                if (Velocity.Y > MAX_VEL_Y)
                    Velocity.Y = MAX_VEL_Y;
            }

            else
                Velocity.Y = 0;

            if (FacingRight)
                Velocity.X = MAX_VEL_X;

            else
                Velocity.X = -MAX_VEL_X;
                
            Pos += Velocity;

            animation.Animate(cAction);

            if (stats.HP <= 0)
            {
                Alive = false;
                Notify(); // tell parent Cube that I died
            }

            HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (FacingRight)
                SE = SpriteEffects.None;

            else
                SE = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(Texture, new Vector2((int)Pos.X, (int)Pos.Y), cAction.Rec[cAction.CurrentFrame], Color.White, 0, cAction.Center, 1, SE, 0);
            //spriteBatch.Draw(Texture, HDRec, new Rectangle(16, 0, 1, 1), new Color(0, 0, 255, 150)); // display hit detection rectangle on-screen
        }
    }
}
