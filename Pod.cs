using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KIC
{
    /// <summary>
    /// A floating Pod that stays in place while firing falling projectiles in 2 different configurations.
    /// Will spin before firing, and the direction it spins will determine how projectiles are fired.
    /// Can either drop a large projectile straight down, or fire 4 small projectiles in a spread formation upwards.
    /// </summary>
    class Pod : Enemy
    {
        static class A // Labels for possible actions (Enumerables are badly designed, this works much nicer)
        {
            static public int Idle = 0;
            static public int TurnRight = 1;
            static public int TurnLeft = 2;
            static public int AttackDown = 3;
            static public int AttackUp = 4;
        }

        private int Count = 0;
        private int Hold = 0;
        private Random random;
        private List<Projectile> Projectiles;

        public Pod() : base()
        {
            Actions.Add(new Action(1, Texture, new Rectangle(0, 0, 16, 16), new Vector2(8, 8), 0, new Rectangle(-8, -8, 16, 16), false, false, 0)); // idle
            Actions.Add(new Action(3, Texture, new Rectangle(116, 0, 16, 16), new Vector2(8, 8), 10, new Rectangle(-8, -8, 16, 16),  true, false, 1)); // turn right
            Actions.Add(new Action(3, Texture, new Rectangle(65, 0, 16, 16), new Vector2(8, 8), 10, new Rectangle(-8, -8, 16, 16),  true, false, 2)); // turn left
            Actions.Add(new Action(1, Texture, new Rectangle(17, 0, 30, 16), new Vector2(15, 8), 0, new Rectangle(-15, -8, 30, 16),  true, false, 3)); // attack down
            Actions.Add(new Action(1, Texture, new Rectangle(48, 0, 16, 23), new Vector2(8, 15), 0, new Rectangle(-8, -15, 16, 23),  true, false, 4)); // attack up
            ID = 0;
            cAction = Actions[A.Idle];
            random = new Random();
            Directional = false; // does not face different directions
            Projectiles = new List<Projectile>();
            stats.SetStats(25, 20, 5); // HP - 25, Attack - 20, Defense - 5
            EnergyDrop = new int[3] { 12, 10, 2 };
        }

        public override void SetRandom(Random R) // initialize random seed
        {  
            random = R;
        }

        public override void Update(Room room, KIC_Object Kic)
        {
            if (stats.HP > 0)
            {
                if (Count <= 0)
                {
                    Count = random.Next(3);

                    if (Count < 2) // 2 in 3 chance of downwards attack
                        cAction = Actions[A.TurnRight];

                    else 
                        cAction = Actions[A.TurnLeft];

                    Count = random.Next(100, 170); // set interval til next attack
                }

                if (cAction.NoLoop && Hold == 0) // if not already attacking
                {
                    if (!animation.AnimateNL(cAction))
                    {
                        if (cAction.Num == A.TurnLeft || cAction.Num == A.TurnRight) // if done turning
                        {
                            cAction = Actions[cAction.Num + 2]; // go to respective attack

                            if (cAction.Num == A.AttackDown)
                                SpawnProjectile(true);

                            else
                                SpawnProjectile(false);
                        }

                        else
                            Hold = 30; // hold attack stance for a bit
                    }
                }

                if (Hold == 0)
                    Count--;

                else
                    Hold--;

                if (Hold == 1) // done holding
                {
                    cAction = Actions[A.Idle];  // set to idle
                    Hold = 0;
                }

                HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);
            }

            foreach (Projectile P in Projectiles) // Projectile.Update()
            {
                if (P.Velocity.Y < 2)
                    P.Velocity.Y += .1f;

                if (P.Type == 1) // for upwards projectiles, there's brief horizontal movement
                {
                    if (P.Velocity.X > 0)
                        P.Velocity.X *= .95f;

                    if (P.Velocity.X < 0)
                        P.Velocity.X *= .95f;
                }

                P.Pos += P.Velocity;
            }

            if (Projectiles.Count > 0)
            {
                Boolean alive = true;

                for (int i = 0; i < Projectiles.Count; i++)
                {
                    if (Projectiles[i].Type == 0) // downwards projectile
                    {
                        if (!Kic.Invincible) // if Kic is not invincible
                        {
                            if (Kic.PRec.Intersects(new Rectangle((int)Projectiles[i].Pos.X - 4, (int)Projectiles[i].Pos.Y - 4, 8, 8)))
                            {   // if normal hit detection is found, or the "filled in" hitbox is being hit
                                if (HitDetection.IntersectingPixels(Kic.PRec, new Rectangle((int)Projectiles[i].Pos.X - 4, (int)Projectiles[i].Pos.Y - 4, 8, 8), new Rectangle(167, 0, 8, 8), Texture))
                                {
                                    if (Projectiles[i].Pos.X > Kic.Pos.X) // projectile is to the right of Kic
                                        Kic.TakeDamage(false, room, stats.STR, 1); // make Kic stagger to the left

                                    else if (Projectiles[i].Pos.X < Kic.Pos.X)
                                        Kic.TakeDamage(true, room, stats.STR, 1);

                                    else
                                        Kic.TakeDamage(!Kic.FacingRight, room, stats.STR, 1); // if right above Kic, stagger backwards

                                    Projectiles.Remove(Projectiles[i]); // delete this projectile
                                    i--;
                                    alive = false;
                                }
                            }
                        }
                    }

                    else // Type == 1, Upwards projectile
                    {
                        if (!Kic.Invincible) // if not invincible
                        {
                            if (Kic.PRec.Intersects(new Rectangle((int)Projectiles[i].Pos.X - 2, (int)Projectiles[i].Pos.Y - 2, 5, 5)))
                            {
                                if (HitDetection.IntersectingPixels(Kic.PRec, new Rectangle((int)Projectiles[i].Pos.X - 2, (int)Projectiles[i].Pos.Y - 2, 5, 5), new Rectangle(167, 9, 5, 5), Texture))
                                {
                                    if (Projectiles[i].Pos.X > Kic.Pos.X)
                                        Kic.TakeDamage(false, room, stats.STR, .75f);

                                    else if (Projectiles[i].Pos.X < Kic.Pos.X)
                                        Kic.TakeDamage(true, room, stats.STR, .75f);

                                    else
                                        Kic.TakeDamage(!Kic.FacingRight, room, stats.STR, .75f);

                                    Projectiles.Remove(Projectiles[i]);
                                    i--;
                                    alive = false;
                                }
                            }
                        }
                    }

                    if (alive) // skip tile check if it already hit Kic
                    {
                        foreach (Tile T in room.Tiles) // do collision checks with floors and/or walls
                        {
                            if (Projectiles[i].Type == 0) // downwards
                            {
                                if (T.Rec.Intersects(new Rectangle((int)Projectiles[i].Pos.X - 4, (int)Projectiles[i].Pos.Y - 4, 8, 8)))
                                {
                                    Projectiles.Remove(Projectiles[i]);
                                    i--;
                                    break;
                                }
                            }

                            else // upwards
                                if (T.Rec.Intersects(new Rectangle((int)Projectiles[i].Pos.X - 2, (int)Projectiles[i].Pos.Y - 2, 5, 5)))
                                {
                                    Projectiles.Remove(Projectiles[i]);
                                    i--;
                                    break;
                                }
                        }
                    }
                }
            }

            if(stats.HP <= 0)
                if (Projectiles.Count <= 0 && stats.HP <= 0) // HP is below 0 and all projectiles are done
                    Alive = false; // this Pod is dead
                 
        }

        public void SpawnProjectile(Boolean FireDown)
        {
            if (FireDown)
                Projectiles.Add(new Projectile(new Vector2(Pos.X, Pos.Y + 5), Vector2.Zero, Texture, 0));
            
            else // firing upwards
            {
                Projectiles.Add(new Projectile(new Vector2(Pos.X - 4, Pos.Y - 4), new Vector2(-1.2f, -2), Texture, 1));
                Projectiles.Add(new Projectile(new Vector2(Pos.X + 4, Pos.Y - 4), new Vector2(1.2f, -2), Texture, 1));
                Projectiles.Add(new Projectile(new Vector2(Pos.X - 4, Pos.Y - 4), new Vector2(-2.3f, -2), Texture, 1));
                Projectiles.Add(new Projectile(new Vector2(Pos.X + 4, Pos.Y - 4), new Vector2(2.3f, -2), Texture, 1));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {                      
            //spriteBatch.Draw(Texture, HDRec, new Rectangle(16, 0, 1, 1), new Color(255, 0, 0, 255)); // (16, 0) on spritesheet is a white pixel <-- HDRec on-screen Display

            if(stats.HP > 0)
                spriteBatch.Draw(Texture, new Vector2((int)Pos.X, (int)Pos.Y), cAction.Rec[cAction.CurrentFrame], Color.White, 0, cAction.Center, 1, SpriteEffects.None, 0);

            foreach (Projectile P in Projectiles)
            {
                if (P.FlickerCount > 3) // projectiles change from Yellow to OrangeRed every 3 frames
                {
                    if (P.Flicker)
                        P.color = Color.OrangeRed;

                    else
                        P.color = Color.Yellow;

                    P.Flicker = !P.Flicker;
                    P.FlickerCount = 0;
                }

                P.FlickerCount++;

                if (P.Type == 0)
                    P.Draw(spriteBatch, new Rectangle(167, 0, 8, 8), new Vector2(4, 4)); // larger projectile
                
                else
                    P.Draw(spriteBatch, new Rectangle(167, 9, 5, 5), new Vector2(2, 2)); // smaller projectile
            }
        }
    }
}
