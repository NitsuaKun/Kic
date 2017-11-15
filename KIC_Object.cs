using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace KIC
{
    class KIC_Object
    {
        public Vector2 Pos;
        public Vector2 Velocity;
        public Boolean FacingRight;
        private Boolean Attacking;
        private Boolean SkipPlatform;
        public Action cAction;
        public List<Action> Actions = new List<Action>();
        public enum A : int { Run, Stand, Jump, WeakStandKick, Crouch, WeakCrouchKick, Backdash, StrongStandKick, SlideKick, WeakJumpKick, StrongJumpKick, Recovery, CrouchSlideKick, GroundPain, Backflip};
        private List<Boolean> IsAttack = new List<Boolean>{false, false, false, true, false, true, false, true, true, true, true, false, true, false}; // which actions are attacks
        private Dictionary<int, Rectangle[]> FindARec; // holds all the hitboxs for applicable attack frames (facing left)
        private Dictionary<int, Rectangle[]> FindARec2; // holds hitboxes for flipped attack frames (facing right)
        private Dictionary<int, int[]> AttackFrame; // helps specify which frame(s) in an action are active attack windows
        public Animation animation = new Animation();
        public Texture2D KicMove;
        public SpriteEffects SP;
        public Boolean Airborn;
        public Boolean Crouching;
        public Boolean JumpHold;
        public int JumpCount;
        public int DashCount;
        public Rectangle HDRec; // hit detection rectangle
        public Rectangle PRec; // platforming rectangle
        public Rectangle ARec; // attack rectangle
        private float MaxVelocityX;
        private float MaxVelocityY;
        private float MaxFall;
        private float MaxJumpCount;
        private float MaxGroundAccel;
        private float MaxAirAccel;
        private float MaxStop;
        private float MaxStopAir;
        private Vector2[] Scarf;
        private Vector2[] ScarfPos;
        private Trig Trig;
        private const int SCARF_LENGTH = 20; // default 20
        private List<Vector2>[] ScarfFrame;
        private Boolean PlatformPass;
        private int PreviousAction;
        private int Window;
        private Boolean KickFirst;
        private Boolean MoveFirst;
        private const int MAX_WINDOW = 6;
        private Boolean RightLast;
        private int Hold;
        private Boolean HoldAction;
        private int Shake;
        private Boolean Flash;
        private int FlashCount;
        private int ShakePos;
        private Boolean LandingJump;
        private const float DEADZONE = .85f;
        public int[] ActionLog;
        public int PlatformPassWindow;
        public SoundEffect Step;
        private ContentManager content;
        private List<Text> Texts;
        private SpriteFont DamageFont;
        public List<Boolean> EnemyCheck;
        public Boolean Invincible;
        private int InvincibleCount;
        private Boolean HoldSlideAttack;
        private int HoldSlideCount;
        public Stats stats;
        private Dictionary<int, float> AtkPow;
        private float ChargePow;
        private float ChargeMax;
        private float DPS;
        private int Time;
        private int TotalDamage;
        private Boolean StartDPS;
        //private Boolean RepeatAttack;
        private int SlideKickDelay;
        private float Rotation;
        private int SlideHold;

        public void LoadActions()
        {
            /*                  Frames     DPS          Pow
            Weak Stand    -     2          1            1
            Weak Jump     -     2          1            1
            Weak Crouch   -     2          1            1
            Strong Stand  -     10         1.2          6
            Charged Stand -     110        1.5          76 (82 x weak attacks, but we're just adding onto the strong stand kick which is already 6x)
            */

            Actions.Add(new Action(8, KicMove, new Rectangle(0, 0, 39, 32), new Vector2(20, 17), 6, new Rectangle(-7, -17, 13, 32), new Rectangle(-3, -15, 5, 30), false, false, 0)); // Run - 0
            Actions.Add(new Action(1, KicMove, new Rectangle(0, 33, 13, 30), new Vector2(7, 15), 1, new Rectangle(-7, -15, 13, 30), new Rectangle(-3, -15, 5, 30), false, false, 1)); // Stand - 1
            Actions.Add(new Action(1, KicMove, new Rectangle(152, 33, 23, 34), new Vector2(12, 15), 1, new Rectangle(-12, -15, 23, 34),  new Rectangle(-3, -15, 5, 30), false, false, 2)); // Jump - 2
            Actions.Add(new Action(3, KicMove, new Rectangle(682, 36, 45, 30), new Vector2(23, 15), 5, new Rectangle(-23, -15, 45, 30), new Rectangle(-3, -15, 5, 30), true, false, 3)); // Weak standing kick - 3
            Actions.Add(new Action(2, KicMove, new Rectangle(176, 33, 23, 30), new Vector2(12, 15), 2, new Rectangle(-12, -15, 23, 30), new Rectangle(-3, -3, 5, 18), false, true, 4)); // Crouch - 4
            Actions.Add(new Action(3, KicMove, new Rectangle(224, 33, 47, 20), new Vector2(24, 5), 5, new Rectangle(-24, -5, 47, 20), new Rectangle(-3, -3, 5, 18), true, false, 5)); // Weak crouch kick - 5
            Actions.Add(new Action(1, KicMove, new Rectangle(224, 54, 29, 28), new Vector2(15, 13), 1, new Rectangle(-15, -13, 29, 28), new Rectangle(-3, -13, 5, 28), true, false, 6)); // Backdash - 6
            Actions.Add(new Action(10, KicMove, new Rectangle(0, 86, 81, 28), new Vector2(41, 13), 6, new Rectangle(-41, -13, 81, 28), new Rectangle(-3, -13, 5, 28), true, false, 7)); // Strong standing kick - 7
            Actions.Add(new Action(2, KicMove, new Rectangle(254, 54, 45, 28), new Vector2(23, 13), 0, new Rectangle(-23, -13, 45, 28), new Rectangle(-3, -13, 5, 28), true, true, 8)); // Slide Kick - 8
            Actions.Add(new Action(3, KicMove, new Rectangle(368, 33, 45, 20), new Vector2(23, 15), 5, new Rectangle(-23, -15, 45, 20), new Rectangle(-3, -15, 5, 20), true, false, 9)); // weak jump kick - 9
            Actions.Add(new Action(8, KicMove, new Rectangle(320, 15, 63, 14), new Vector2(32, 15), 6, new Rectangle(-32, -15, 63, 14), new Rectangle(-3, -8, 5, 7), true, false, 10)); // strong jump kick - 10
            Actions.Add(new Action(2, KicMove, new Rectangle(346, 54, 23, 30), new Vector2(12, 15), 6,  new Rectangle(-12, -15, 23, 30), new Rectangle(-3, -3, 5, 18), true, false, 11)); // recovery - 11
            Actions.Add(new Action(1, KicMove, new Rectangle(320, 33, 47, 20), new Vector2(24, 5), 1, new Rectangle(-24, -5, 47, 20), new Rectangle(-3, -3, 5, 18), false, false, 12)); // crouch slidekick -12
            //Actions.Add(new Action(8, KicMove, new Rectangle(320, 0, 62, 14), new Vector2(32, -1), 6, new Rectangle(-32, 1, 62, 14), new Rectangle(-3, 1, 5, 14), true, false, 12)); // strong crouch kick - 12
            Actions.Add(new Action(1, KicMove, new Rectangle(394, 54, 29, 28), new Vector2(15, 13), 1, new Rectangle(0, 0, 0, 0), new Rectangle(-3, -13, 5, 28), false, false, 13)); // pain - 13
            Actions.Add(new Action(8, KicMove, new Rectangle(0, 115, 25, 25), new Vector2(13, 13), 5, new Rectangle(-13, -13, 25, 25), new Rectangle(-3, -10, 5, 19), false, false, 14)); // backflip - 14

            AtkPow = new Dictionary<int, float>();
            float StrongPow = (Actions[7].Frames / Actions[3].Frames) * 1.5f;
            ChargeMax = ((Actions[7].Frames + 200) / Actions[7].Frames);
            AtkPow.Add(3, 1); // W stand
            AtkPow.Add(5, 1); // W crouch
            AtkPow.Add(7, StrongPow); // S Stand
            AtkPow.Add(8, 1); // Slide
            AtkPow.Add(9, 1); // W Jump
            AtkPow.Add(10, StrongPow); // S Jump
            AtkPow.Add(12, 1); // Crouch Slide
            //AtkPow.Add(12, StrongPow); // S crouch

            FindARec = new Dictionary<int, Rectangle[]>();
            FindARec2 = new Dictionary<int, Rectangle[]>();
            AttackFrame = new Dictionary<int, int[]>();

            //FindARec.Add(3, new Rectangle[] { new Rectangle(-23, -13, 16, 28) });  // weak stand kick (left)
            //FindARec2.Add(3, new Rectangle[] { new Rectangle(6, -13, 16, 28) });  // right
            FindARec.Add(3, new Rectangle[] { new Rectangle(-23, -8, 16, 5) });  // weak stand kick (left)
            FindARec2.Add(3, new Rectangle[] { new Rectangle(6, -8, 16, 5) });  // right
            AttackFrame.Add(3, new int[] { 2 });
            FindARec.Add(5, new Rectangle[] { new Rectangle(-24, -5, 19, 20) });  // weak crouch kick (left)
            FindARec2.Add(5, new Rectangle[] { new Rectangle(4, -5, 19, 20) });  // right
            AttackFrame.Add(5, new int[] { 2 });
            FindARec.Add(7, new Rectangle[] { new Rectangle(-41, -13, 22, 28), new Rectangle(-41, -13, 22, 28) });  // strong stand kick (left)
            FindARec2.Add(7, new Rectangle[] { new Rectangle(18, -13, 22, 28), new Rectangle(18, -13, 22, 28) }); // right
            AttackFrame.Add(7, new int[] { 5, 6 });
            FindARec.Add(8, new Rectangle[] { new Rectangle(-23, -13, 16, 28) });  // slide kick (left)
            FindARec2.Add(8, new Rectangle[] { new Rectangle(6, -13, 16, 28) });  // right
            AttackFrame.Add(8, new int[] { 1 });
            FindARec.Add(9, new Rectangle[] { new Rectangle(-23, -15, 18, 20) });  // weak jump kick (left)
            FindARec2.Add(9, new Rectangle[] { new Rectangle(4, -15, 18, 20) });  // right
            AttackFrame.Add(9, new int[] { 2 });
            FindARec.Add(10, new Rectangle[] { new Rectangle(-32, -15, 28, 14), new Rectangle(-32, -15, 28, 14) }); // strong jump kick (left)
            FindARec2.Add(10, new Rectangle[] { new Rectangle(3, -15, 28, 14), new Rectangle(3, -15, 28, 14) }); // right
            AttackFrame.Add(10, new int[] { 3, 4 });
            FindARec.Add(12, new Rectangle[] { new Rectangle(-24, -5, 19, 20) });  // crouch slidekick (left)
            FindARec2.Add(12, new Rectangle[] { new Rectangle(4, -5, 19, 20) });  // right
            AttackFrame.Add(12, new int[] { 0 });
            //FindARec.Add(12, new Rectangle[] { new Rectangle(-32, 1, 28, 14), new Rectangle(-32, 1, 28, 14) }); // strong crouch kick
            //FindARec2.Add(12, new Rectangle[] { new Rectangle(2, 1, 28, 14), new Rectangle(2, 1, 28, 14) });
            //AttackFrame.Add(12, new int[] { 3, 4 });

            // remember: when measuring the adjusted X position of the flipped sprite, take off 1!

            ScarfFrame = new List<Vector2>[Actions.Count];
            for (int i = 0; i < Actions.Count; i++)
                ScarfFrame[i] = new List<Vector2>();

            ScarfFrame[0].Add(new Vector2(2, -13)); // running
            ScarfFrame[0].Add(new Vector2(2, -14));
            ScarfFrame[0].Add(new Vector2(2, -15));
            ScarfFrame[0].Add(new Vector2(2, -14));
            ScarfFrame[0].Add(new Vector2(2, -13));
            ScarfFrame[0].Add(new Vector2(2, -14));
            ScarfFrame[0].Add(new Vector2(2, -15));
            ScarfFrame[0].Add(new Vector2(2, -14));

            ScarfFrame[1].Add(new Vector2(2, -13)); // standing

            ScarfFrame[2].Add(new Vector2(2, -13)); // jump

            ScarfFrame[3].Add(new Vector2(2, -11)); // weak stand kick
            ScarfFrame[3].Add(new Vector2(2, -11));
            ScarfFrame[3].Add(new Vector2(2, -11));
            //ScarfFrame[3].Add(new Vector2(2, -11));

            ScarfFrame[4].Add(new Vector2(2, -13)); // crouch
            ScarfFrame[4].Add(new Vector2(2, -2));

            ScarfFrame[5].Add(new Vector2(2, -2)); // weak crouch kick
            ScarfFrame[5].Add(new Vector2(2, -2));
            ScarfFrame[5].Add(new Vector2(2, -2));

            ScarfFrame[6].Add(new Vector2(2, -11)); // backdash

            ScarfFrame[7].Add(new Vector2(0, -11)); // strong stand kick
            ScarfFrame[7].Add(new Vector2(-2, -10));
            ScarfFrame[7].Add(new Vector2(-4, -9));
            ScarfFrame[7].Add(new Vector2(-6, -7));
            ScarfFrame[7].Add(new Vector2(-6, -5));
            ScarfFrame[7].Add(new Vector2(-12, -7));
            ScarfFrame[7].Add(new Vector2(-12, -7));
            ScarfFrame[7].Add(new Vector2(-4, -8));
            ScarfFrame[7].Add(new Vector2(2, -9));
            ScarfFrame[7].Add(new Vector2(2, -11));

            ScarfFrame[8].Add(new Vector2(2, -11)); // slide kick
            ScarfFrame[8].Add(new Vector2(2, -11));

            ScarfFrame[9].Add(new Vector2(2, -13)); // weak jumpkick
            ScarfFrame[9].Add(new Vector2(2, -13));
            ScarfFrame[9].Add(new Vector2(2, -13));

            ScarfFrame[10].Add(new Vector2(-3, -5)); // strong jumpkick
            ScarfFrame[10].Add(new Vector2(-3, -5));
            ScarfFrame[10].Add(new Vector2(-3, -5));
            ScarfFrame[10].Add(new Vector2(-3, -5));
            ScarfFrame[10].Add(new Vector2(-3, -5));
            ScarfFrame[10].Add(new Vector2(-3, -5));
            ScarfFrame[10].Add(new Vector2(-3, -5));
            ScarfFrame[10].Add(new Vector2(-3, -5));

            ScarfFrame[11].Add(new Vector2(2, -2)); // recovery
            ScarfFrame[11].Add(new Vector2(2, -13));

            ScarfFrame[12].Add(new Vector2(2, -2)); // crouch slidekick

            ScarfFrame[13].Add(new Vector2(2, -13)); // ground pain

            ScarfFrame[14].Add(new Vector2(9, -8)); // backflip
            ScarfFrame[14].Add(new Vector2(11, -2));
            ScarfFrame[14].Add(new Vector2(8, 9));
            ScarfFrame[14].Add(new Vector2(2, 11));
            ScarfFrame[14].Add(new Vector2(-9, 8));
            ScarfFrame[14].Add(new Vector2(-11, 2));
            ScarfFrame[14].Add(new Vector2(-8, -9));
            ScarfFrame[14].Add(new Vector2(-2, -11));
        }

        /// <summary>
        /// Spawns Kic onto the screen
        /// </summary>
        public KIC_Object(Texture2D T, ScreenManager SM)
        {
            KicMove = T;
            Airborn = false;
            JumpHold = false;
            JumpCount = 0;
            Pos.X = 0;
            Pos.Y = 0;
            HDRec = new Rectangle(0, 0, 0, 0);
            MaxVelocityX = 3;
            MaxVelocityY = 3;
            MaxFall = .12f;
            MaxJumpCount = 15;
            MaxGroundAccel = .3f;
            MaxAirAccel = .3f;
            MaxStop = .4f;
            MaxStopAir = .4f;
            Scarf = new Vector2[SCARF_LENGTH];
            ScarfPos = new Vector2[SCARF_LENGTH];
            Trig = new Trig();
            Hold = 0;
            Shake = 0;
            ActionLog = new int[5];
            content = new ContentManager(SM.Game.Services, "Content");
            Step = content.Load<SoundEffect>("Audio//Sound//Footsteps");
            Texts = new List<Text>();
            DamageFont = content.Load<SpriteFont>("Test4");
            EnemyCheck = new List<Boolean>();
            Invincible = false;
            HoldSlideAttack = false;
            stats = new Stats();
            stats.SetStats(50, 10, 10);
            ChargePow = 0;
            StartDPS = false;
            TotalDamage = 0;
            SlideKickDelay = 0;
        }

        public void HandleInput(Room CurrentRoom)
        {
            //if (GP.IsButtonDown(Buttons.RightStick))
            //{
            //    StartDPS = false;
            //    TotalDamage = 0;
            //    Time = 0;
            //}

            if (cAction.Num != (int)A.GroundPain && cAction.Num != (int)A.Backflip) // adding blackflip here makes it so that you can't "glide" by rapidly kicking and backflipping
            {
                //if (cAction.Num == (int)A.WeakStandKick || cAction.Num == (int)A.WeakCrouchKick || cAction.Num == (int)A.WeakJumpKick)
                //{
                //    if(cAction.CurrentFrame >= cAction.Frames - 1)
                //        if (GP.IsButtonDown(Buttons.X) && PGP.IsButtonUp(Buttons.X))  // if weak attack, repeat it immediately if pressed during current weak attack
                //            RepeatAttack = true;
                //}

                //else
                //    RepeatAttack = false;

                if (Controls.StrongKick(true, false) && cAction.Num != (int)A.StrongStandKick && cAction.Num != (int)A.StrongJumpKick && cAction.Num != (int)A.CrouchSlideKick
                    && cAction.Num != (int)A.WeakStandKick && cAction.Num != (int)A.WeakJumpKick && cAction.Num != (int)A.WeakCrouchKick && cAction.Num != (int)A.SlideKick)
                {
                    if (!Airborn)
                    {
                        if (cAction.Num != (int)A.Crouch)
                            Hold = -1;  // flag for charging
                        StrongKick();
                    }

                    else
                    {
                        if (Velocity.Y >= 0) // if moving downwards
                        {  // check to see if Kic is within 5 of any tiles
                            int Total = 0;
                            Boolean AbleToKick = true;
                            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);

                            foreach (Rectangle Rec in CurrentRoom.TileRecs)
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total < 10 && Total >= 0)
                                    {
                                        AbleToKick = false;  // too close to the ground, don't even bother doing the kick
                                        break;
                                    }
                                }
                            }

                            foreach (Rectangle Rec in CurrentRoom.EnemyRecs)
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total < 10 && Total >= 0)
                                    {
                                        AbleToKick = false;  // too close to the ground, don't even bother doing the kick
                                        break;
                                    }
                                }
                            }

                            foreach (Rectangle Rec in CurrentRoom.PlatformRecs)
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total < 10 && Total >= 0)
                                    {
                                        AbleToKick = false;  // too close to the ground, don't even bother doing the kick
                                        break;
                                    }
                                }
                            }
                            if (AbleToKick)
                                StrongKick();
                        }

                        else
                            StrongKick();
                    }
                }

                else if (Controls.StrongKick(false, false) && Hold != 0)
                {
                    if (cAction.CurrentFrame == 4) // Kic is now fully wound up, charge it if C has been held til now
                    {
                        HoldAction = true;
                        if (Hold == -1)
                            Hold = 0;

                        if (Hold < 200) // hold it for 100 frames
                        {
                            Hold++;
                            Shake++;
                            if (Hold % 3 == 0) // will shake every 4 frames
                            {
                                if (FacingRight)
                                {
                                    if ((Hold / Shake) % 2 != 0)
                                        ShakePos = 1;
                                    else
                                        ShakePos = 0;
                                }

                                else
                                {
                                    if ((Hold / Shake) % 2 != 0)
                                        ShakePos = -1;
                                    else
                                        ShakePos = 0;
                                }
                                Shake = 0;
                            }
                        }

                        if (Hold == 199)
                        {
                            Hold++;
                            Flash = true;
                            FlashCount = 0;
                            ShakePos = 0;
                        }
                    }
                }

                if (Controls.StrongKick(false, true)) // was just released
                {
                    if (Hold == -1) // cancel charging
                        Hold = 0;
                    else
                    {
                        if (cAction.Num == (int)A.StrongStandKick)
                            ChargePow = ChargeMax * ((float)Hold / 200f);
                        HoldAction = false;
                        ShakePos = 0;
                    }
                }

                if (cAction.Num != (int)A.StrongStandKick) // can't do ANYTHING during a strong attack
                {
                    if (!Airborn && SlideKickDelay <= 0)
                    {
                        if (Controls.WeakKick(true) && cAction.Num != (int)A.WeakStandKick && cAction.Num != (int)A.WeakCrouchKick && cAction.Num != (int)A.SlideKick)
                        {                                                    
                                if (!MoveFirst) // this came before movement
                                {
                                    KickFirst = true;
                                    Window = 0;
                                }

                                else if (Window < MAX_WINDOW && SlideKickDelay <= 0) // move was pressed first and still within slidekick window
                                {
                                    HoldAction = true;
                                    Hold = 0;
                                    Shake = 0;
                                    SlideKick();
                                    KickFirst = false;
                                }
      
                            if (cAction.Num != (int)A.SlideKick)
                                WeakKick();
                        }

                        if (KickFirst)
                        {
                            if (Window >= MAX_WINDOW)
                            {
                                KickFirst = false;
                            }

                            if (Window < MAX_WINDOW)
                                Window++;
                        }
                    }

                    else if (Controls.WeakKick(true) && cAction.Num != (int)A.WeakJumpKick && cAction.Num != (int)A.StrongJumpKick) // is airborn
                    {
                        if (Velocity.Y >= 0) // if moving downwards
                        {  // check to see if Kic is within 5 of any tiles
                            int Total = 0;
                            Boolean AbleToKick = true;
                            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);

                            foreach (Rectangle Rec in CurrentRoom.TileRecs)
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total < 5 && Total >= 0)
                                    {
                                        AbleToKick = false;  // too close to the ground, don't even bother doing the kick
                                        break;
                                    }
                                }
                            }

                            foreach (Rectangle Rec in CurrentRoom.EnemyRecs)
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total < 5 && Total >= 0)
                                    {
                                        AbleToKick = false;  // too close to the ground, don't even bother doing the kick
                                        break;
                                    }
                                }
                            }

                            foreach (Rectangle Rec in CurrentRoom.PlatformRecs)
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total < 5 && Total >= 0)
                                    {
                                        AbleToKick = false;  // too close to the ground, don't even bother doing the kick
                                        break;
                                    }
                                }
                            }
                            if (AbleToKick)
                                WeakKick();
                        }

                        else
                            WeakKick();
                    }

                    if(!Controls.Down(false))
                    if (((!Controls.Left(false, false) && !Controls.Right(false, false)) && cAction.Num != (int)A.WeakStandKick && cAction.Num <= (int)A.Crouch) || // the <= leaves the jumpkicks out of this
                        ((!Controls.Left(false, false) && !Controls.Right(false, false)) && (cAction.Num == (int)A.WeakJumpKick || cAction.Num == (int)A.StrongJumpKick))) // this can be fixed, once we rearrange all the actions
                        if(cAction.Num != (int)A.GroundPain)
                            SlowDown(CurrentRoom);

                    if (cAction.Num == (int)A.SlideKick)
                    {
                        if (Controls.WeakKick(false))
                        {
                            if (Hold < 100) // hold it for 100 frames
                            {
                                Hold++;
                                Shake++;
                                if (Hold % 3 == 0) // will shake every 4 frames
                                {
                                    if (FacingRight)
                                    {
                                        if ((Hold / Shake) % 2 != 0)
                                            ShakePos = -1;
                                        else
                                            ShakePos = 0;
                                    }

                                    else
                                    {
                                        if ((Hold / Shake) % 2 != 0)
                                            ShakePos = 1;
                                        else
                                            ShakePos = 0;
                                    }
                                    Shake = 0;
                                }
                            }

                            if (Hold == 99)
                            {
                                Hold++;
                                Flash = true;
                                FlashCount = 0;
                                ShakePos = 0;
                            }
                        }
                        else
                        {
                            HoldAction = false;
                            cAction.CurrentFrame = 1;
                            ShakePos = 0;
                        }
                    }

                    if (cAction.Num != (int)A.Backflip && cAction.Num != (int)A.Backdash && cAction.Num != (int)A.SlideKick && cAction.Num != (int)A.CrouchSlideKick && (cAction.Num != (int)A.WeakStandKick || KickFirst))
                    {
                        if (Controls.Left(false, false))
                        {
                            if (!KickFirst) // movement started before kick, initiate possible slidekick
                            {
                                if (!Controls.Left(false, true))
                                {
                                    Window = 0;
                                    MoveFirst = true;
                                }
                            }

                            else if (!Controls.Left(false, true) && Window < MAX_WINDOW) // kick was pressed first and still within slidekick window
                            {
                                Hold = 0;
                                Shake = 0;
                                HoldAction = true;
                                FacingRight = false;
                                SlideKick();
                            }

                            Move(false, CurrentRoom);
                        }

                        if (Controls.Right(false, false))
                        {
                            if (!KickFirst) // movement started before kick, initiate possible slidekick
                            {
                                if (!Controls.Right(false, true))
                                {
                                    Window = 0;
                                    MoveFirst = true;
                                }
                            }

                            else if (!Controls.Right(false, true) && Window < MAX_WINDOW) // kick was pressed first and still within slidekick window
                            {
                                FacingRight = true;
                                Hold = 0;
                                HoldAction = true;
                                SlideKick();
                            }

                            Move(true, CurrentRoom);
                        }

                        //GP.ThumbSticks.Left.x < -DEADZONE
                        //GP.ThumbSticks.Left.Y >= DEADZONE

                        if ((!Controls.Left(false, false) && !Controls.Right(false, false)))
                            if (Controls.Down(false) && !Airborn && cAction.Num != (int)A.WeakCrouchKick && cAction.Num != (int)A.CrouchSlideKick)
                                Crouch();
                    }

                    if (MoveFirst)
                    {
                        if (Window >= MAX_WINDOW)
                            MoveFirst = false;
                        if (Window < MAX_WINDOW)
                            Window++;
                    }

                    if (Controls.Dodge(true) && !Airborn && cAction.Num == (int)A.Crouch)
                    {
                        PlatformPass = true;
                        PlatformPassWindow = 12;
                        Velocity.Y = MaxVelocityY;
                    }

                    if (!Airborn && (Controls.Jump(true, false) ||
                        //(PGP.IsButtonDown(Buttons.Space) && LandingJump)) // save this line for double jumps
                        (Controls.Jump(false, true) && LandingJump)))// start the jump
                    {
                        if (cAction.Num != (int)A.Crouch)
                            Jump(true);

                        else
                        {
                            PlatformPass = true;
                            PlatformPassWindow = 12;
                        }

                        LandingJump = false;
                    }

                    else if (Controls.Jump(false, false) && Controls.Jump(false, true) && JumpHold) // continue the jump **check both current and previous controller states for being pressed
                    {
                        if (cAction.Num != (int)A.Crouch) // if not crouching
                            Jump(false);
                    }

                    else if (!Controls.Jump(false, false)) // end the jump
                    {
                        JumpHold = false;
                        if (Controls.Jump(false, true))
                            if (Velocity.Y < 0)
                                Velocity.Y /= 2;
                    }

                    if (!JumpHold && Airborn && Controls.Jump(true, false)) // jump has ended, but player wants next one to happen upon landing
                    {
                        LandingJump = true;
                    }

                    if (Airborn && Controls.Down(false))
                        SkipPlatform = true;
                    else
                        SkipPlatform = false;


                    if (Controls.Dodge(true))
                    {
                        if (!Airborn && cAction.Num != (int)A.Backdash)
                            Backdash();
                        else if (Airborn && cAction.Num != (int)A.Backflip)
                            Backflip();
                    }
                }
            }

            else if (cAction.Num == (int)A.Backflip) // since all the above code excludes backflips to prevent the hovering kick glitch, need to add this so you can control when to fall through platforms when backflipping
            {
                if (Airborn && Controls.Down(false))
                    SkipPlatform = true;
                else
                    SkipPlatform = false;
            }

            if (cAction.Num != PreviousAction)
            {
                cAction.CurrentFrame = 0;
                cAction.SpeedCount = 0;

                ActionLog[4] = ActionLog[3];
                ActionLog[3] = ActionLog[2];
                ActionLog[2] = ActionLog[1];
                ActionLog[1] = ActionLog[0];
                ActionLog[0] = cAction.Num;
            }

            PreviousAction = cAction.Num;

            Update(CurrentRoom);
        }

        public void Update(Room R)
        {
            if (PlatformPassWindow > 0)
                PlatformPassWindow--;

            if (cAction.Num != (int)A.SlideKick && cAction.Num != (int)A.StrongStandKick)  // to clear up any slight bugs that might stick around
            {
                HoldAction = false;
                ShakePos = 0;
            }

            if (!HoldAction)
            {
                if (cAction.Frames > 1 && !cAction.NoLoop && !cAction.Hold)
                    Animate();
                if (cAction.NoLoop)
                {
                    if (!AnimateNL())
                    {
                        if (cAction.Num == (int)A.WeakStandKick || cAction.Num == (int)A.StrongStandKick || cAction.Num == (int)A.Recovery)
                        {
                            //if (RepeatAttack)
                            //{
                            //    WeakKick();
                            //    RepeatAttack = false;
                            //}
                            //else
                                cAction = Actions[(int)A.Stand]; // set him to standing after it is done
                            ChargePow = 0; // reset charge
                        }

                        if (cAction.Num == (int)A.WeakJumpKick || cAction.Num == (int)A.StrongJumpKick || cAction.Num == (int)A.Backflip)
                        {
                            int Total = 0;
                            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);

                            foreach (Rectangle Rec in R.TileRecs)  // first check to see if Kic's feet are currently right above any solid ground
                            {
                                if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                {
                                    Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                    if (Total <= Actions[(int)A.Stand].PRec.Height - cAction.PRec.Height && Total >= 0)  // if Kic's legs would end up going through any solid ground
                                    {
                                        if (cAction.Num == (int)A.WeakJumpKick)
                                        {
                                            cAction = Actions[(int)A.Stand];  // set him to stand and position him right on top of this ground
                                            Pos.Y = Rec.Y + cAction.PRec.Y;
                                        }
                                        else
                                        {
                                            cAction = Actions[(int)A.Stand];
                                            Pos.Y = Rec.Y + cAction.PRec.Y;
                                            cAction = Actions[(int)A.Recovery];
                                            Velocity.X = 0;
                                        }
                                                                           
                                        Velocity.Y = 0;
                                        break;
                                    }
                                }
                            }

                            if (Velocity.Y != 0)
                            {
                                foreach (Rectangle Rec in R.PlatformRecs)  // first check to see if Kic's feet are currently right above any solid ground
                                {
                                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                    {
                                        Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                        if (Total <= Actions[(int)A.Stand].PRec.Height - cAction.PRec.Height && Total >= 0)  // if Kic's legs would end up going through any solid ground
                                        {
                                            if (cAction.Num == (int)A.WeakJumpKick)
                                            {
                                                cAction = Actions[(int)A.Stand];  // set him to stand and position him right on top of this ground
                                                Pos.Y = Rec.Y + cAction.PRec.Y;
                                            }
                                            else
                                            {
                                                cAction = Actions[(int)A.Stand];
                                                Pos.Y = Rec.Y + cAction.PRec.Y;
                                                cAction = Actions[(int)A.Recovery];
                                                Velocity.X = 0;
                                            }

                                            Velocity.Y = 0;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (Velocity.Y != 0)
                            {
                                foreach (Rectangle Rec in R.EnemyRecs)  // first check to see if Kic's feet are currently right above any solid ground
                                {
                                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                                    {
                                        Total = (Rec.Y) - (PRec.Y + PRec.Height);
                                        if (Total <= Actions[(int)A.Stand].PRec.Height - cAction.PRec.Height && Total >= 0)  // if Kic's legs would end up going through any solid ground
                                        {
                                            if (cAction.Num == (int)A.WeakJumpKick)
                                            {
                                                cAction = Actions[(int)A.Stand];  // set him to stand and position him right on top of this ground
                                                Pos.Y = Rec.Y + cAction.PRec.Y;
                                            }
                                            else
                                            {
                                                cAction = Actions[(int)A.Stand];
                                                Pos.Y = Rec.Y + cAction.PRec.Y;
                                                cAction = Actions[(int)A.Recovery];
                                                Velocity.X = 0;
                                            }

                                            Velocity.Y = 0;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (cAction.Num != (int)A.Stand) // otherwise, he's still very well airborn, resume jumping
                            {
                                //if (RepeatAttack)
                                //{
                                //    WeakKick();
                                //    RepeatAttack = false;
                                //}

                                //else
                                    cAction = Actions[(int)A.Jump];
                            }
                        }

                        else if (cAction.Num == (int)A.WeakCrouchKick) // if crouching kick
                        {
                            //if (RepeatAttack)
                            //{
                             //   Crouching = true;
                             //   WeakKick();
                             //   RepeatAttack = false;
                            //}

                            //else
                            //{
                                cAction = Actions[(int)A.Crouch]; // set to crouching after
                                cAction.CurrentFrame = 1; // and also skip the first crouching frame
                                PreviousAction = cAction.Num; // and make it so that the frames arent reset because a new action has started
                            //}
                        }
                        PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                    }
                }
                if (cAction.Hold)
                    AnimateHold();
            }

            if (cAction.Num != (int)A.WeakJumpKick && cAction.Num != (int)A.StrongJumpKick)  // note to self: this is what screwed up the entire jumpkick tile clipping
            { // it was setting airborn to false when Kic landed on a tile while jumpkicking, which caused Fall to be skipped entirely
              // and then on the next frame Kic is set to the jumping animation, which puts his feet past the tile he just hit last frame
              // and kic just continues to fall like nothing was there, aside from a slight pause since Velocity.Y is set to 0
                Boolean Check = false;

                if (!SkipPlatform && !PlatformPass)
                {
                    foreach (Rectangle Rec in R.PlatformRecs)
                    {
                        PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                        if (PRec.X > Rec.X - PRec.Width && PRec.X < Rec.X + Rec.Width)
                            if (PRec.Y + PRec.Height == Rec.Y && Rec.Y + Rec.Height > PRec.Y && Velocity.Y >= 0)
                            {
                                Check = true;
                                Airborn = false;
                                Velocity.Y = 0;
                                if (cAction.Num == (int)A.GroundPain) // this was put in to prevent a wierd glitch where if Kic jumped right before getting hit Kic would end up being stuck in stagger mode and keep sliding to the side forever
                                {
                                    cAction = Actions[(int)A.Stand];
                                    InvincibleCount = 30;
                                }
                                if (cAction.Num == (int)A.Backflip)
                                {
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                }
                                break;
                            }
                    }
                }

                if (!Check)
                {
                    foreach (Rectangle Rec in R.TileRecs)
                    {
                        PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                        if (PRec.X > Rec.X - PRec.Width && PRec.X < Rec.X + Rec.Width)
                            if (PRec.Y + PRec.Height == Rec.Y && Rec.Y + Rec.Height > PRec.Y && Velocity.Y >= 0)
                            {
                                Check = true;
                                Airborn = false;
                                Velocity.Y = 0;
                                if (cAction.Num == (int)A.GroundPain) // this was put in to prevent a wierd glitch where if Kic jumped right before getting hit Kic would end up being stuck in stagger mode and keep sliding to the side forever
                                {
                                    cAction = Actions[(int)A.Stand];
                                    InvincibleCount = 30;
                                }
                                if (cAction.Num == (int)A.Backflip)
                                {
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                }
                                break;
                            }
                    }
                }

                if (!Check)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                        if (PRec.X > Rec.X - PRec.Width && PRec.X < Rec.X + Rec.Width)
                            if (PRec.Y + PRec.Height == Rec.Y && Rec.Y + Rec.Height > PRec.Y && Velocity.Y >= 0)
                            {
                                Check = true;
                                Airborn = false;
                                Velocity.Y = 0;
                                if (cAction.Num == (int)A.GroundPain) // this was put in to prevent a wierd glitch where if Kic jumped right before getting hit Kic would end up being stuck in stagger mode and keep sliding to the side forever
                                {
                                    cAction = Actions[(int)A.Stand];
                                    InvincibleCount = 30;
                                }
                                if (cAction.Num == (int)A.Backflip)
                                {
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                }
                                break;
                            }
                    }
                }

                if (!Check)
                {
                    Airborn = true;
                    if (PlatformPass)
                        Pos.Y++;
                }

                PlatformPass = false;
            }

            if (Airborn)
                Fall(R);

            if (cAction.Num == (int)A.Backdash || cAction.Num == (int)A.SlideKick || cAction.Num == (int)A.CrouchSlideKick || cAction.Num == (int)A.GroundPain || cAction.Num == (int)A.Backflip)
                Move(false, R);

            if (cAction.Num != (int)A.Crouch && cAction.Num != (int)A.Backdash && cAction.Num != (int)A.CrouchSlideKick)
                Crouching = false;

            Vector2 Pos1, Pos2;

            if(FacingRight)
                Scarf[0].X = Pos.X + (ScarfFrame[cAction.Num][cAction.CurrentFrame].X * -1) - 4; // the -4 here is to compensate for the rectangle's default pos in the upper left corner
            else
                Scarf[0].X = Pos.X + ScarfFrame[cAction.Num][cAction.CurrentFrame].X;

            Scarf[0].Y = Pos.Y + ScarfFrame[cAction.Num][cAction.CurrentFrame].Y; // Y pos is the same regardless of FacingRight

            for (int k = 1; k < SCARF_LENGTH; k++) // gravity for the scarf (except the one attached to Kic, obviously)
            {
                if (!R.OnSolidGround(new Rectangle((int)Scarf[k].X, (int)Scarf[k].Y, 3, 3), .5f))
                {
                    float tempV = .5f;
                    R.CheckTileCollision(ref tempV, new Rectangle((int)Scarf[k].X, (int)Scarf[k].Y, 3, 3), ref Scarf[k]);
                    Scarf[k].Y += tempV;
                }

                //if (!R.CheckTileCollision(new Rectangle((int)Scarf[k].X, (int)Scarf[k].Y, 3, 3))) // only be effected by gravity if not on/in a tile
                    //Scarf[k].Y += .5f;
            }

            for (int i = 1; i < SCARF_LENGTH; i++)
            {
                Pos1 = new Vector2(Scarf[i].X, Scarf[i].Y);
                Pos2 = new Vector2(Scarf[i - 1].X, Scarf[i - 1].Y);

                if (Scarf[0] != ScarfPos[0])
                {
                    if (Vector2.Distance(Pos1, Pos2) <= 1 && (Scarf[i - 1].X != ScarfPos[i - 1].X || Scarf[i - 1].Y != ScarfPos[i - 1].Y))
                    {
                        Scarf[i].X = ScarfPos[i - 1].X;
                        Scarf[i].Y = ScarfPos[i - 1].Y;
                    }
                }

                if (Vector2.Distance(Pos1, Pos2) > 1)
                {
                    Scarf[i].X = Pos2.X + ((float)Math.Cos(Trig.FindAngle(Pos1, Pos2)) * 1);
                    Scarf[i].Y = Pos2.Y + ((float)Math.Sin(Trig.FindAngle(Pos1, Pos2)) * 1);
                }
            }

            for (int i = 0; i < SCARF_LENGTH; i++)
            {
                ScarfPos[i].X = Scarf[i].X;
                ScarfPos[i].Y = Scarf[i].Y;
            }

            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
            HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);

            //foreach (Enemy E in R.Enemies)
            //{
            //    if (HDRec.Intersects(E.HDRec))
            //    {
            //        if (HitDetection.IntersectingPixels(HDRec, cAction.Rec[cAction.CurrentFrame], KicMove, 
            //                                            E.HDRec, E.cAction.Rec[E.cAction.CurrentFrame], E.Texture))  
            //            Flash = true;                  
            //    }
            //}

            if (FindARec.ContainsKey(cAction.Num) && AttackFrame[cAction.Num].Contains(cAction.CurrentFrame)) // if action is an attack & current frame is an active attack frame
            {
                if (FacingRight) // grab the correct rectangle based on which direction Kic is facing
                    ARec = new Rectangle((int)Pos.X + FindARec2[cAction.Num][0].X, (int)Pos.Y + FindARec2[cAction.Num][0].Y, FindARec2[cAction.Num][0].Width, FindARec2[cAction.Num][0].Height);
                else
                    ARec = new Rectangle((int)Pos.X + FindARec[cAction.Num][0].X, (int)Pos.Y + FindARec[cAction.Num][0].Y, FindARec[cAction.Num][0].Width, FindARec[cAction.Num][0].Height);

                if (!Attacking) // only do this if just starting to attack
                {
                    Attacking = true;
                    EnemyCheck = new List<Boolean>();
                    foreach (Enemy E in R.Enemies) // makes a new list of all false for enemy checking
                        EnemyCheck.Add(false);
                }
            }
            // note to self: if we do end up with any attacks with multiple valid attack frames that have different rectangles, then we need to change the above if/else to account for that
            else
                Attacking = false;
                   
            foreach (Enemy E in R.Enemies)
            {
                if (Attacking && AtkPow.ContainsKey(cAction.Num))
                {
                    if (EnemyCheck.Count < R.Enemies.Count)
                        EnemyCheck.Add(false);

                    if (!EnemyCheck[R.Enemies.IndexOf(E)]) // if this enemy isn't already hit by this attack
                    {
                        if (!E.ComplexHitBox)
                        {
                            if (ARec.Intersects(E.HDRec))
                            {
                                //if (HitDetection.IntersectingPixels(ARec, cAction.Rec[cAction.CurrentFrame], KicMove, FacingRight, E.HDRec, E.cAction.Rec[E.cAction.CurrentFrame], E.Texture))
                                if (HitDetection.IntersectingPixels(ARec, cAction.Rec[cAction.CurrentFrame], cAction.Center, GetFindARec(), KicMove, FacingRight, E.HDRec, E.cAction.Rec[E.cAction.CurrentFrame], E.Texture))
                                {
                                    int Damage = (int)(BattleCalculator.GetDamage(stats.STR, E.stats.DEF) * (AtkPow[cAction.Num] + ChargePow));

                                    Texts.Add(new Text(new Vector2(E.Pos.X - DamageFont.MeasureString(Damage.ToString()).X / 2, E.Pos.Y - E.cAction.Center.Y - 7), new Vector2(0, -.5f), Color.Gray, 60, DamageFont, Damage.ToString()));
                                    E.stats.HP -= Damage;

                                    if (E.stats.HP > 0)
                                    {
                                        E.Effect = SpriteEffectHolder.RedTint;
                                        E.Flash = true;
                                        E.FlashCount = 0;
                                    }

                                    if (!StartDPS)
                                    {
                                        StartDPS = true;
                                    }
                                    else
                                        TotalDamage += Damage;

                                    EnemyCheck[R.Enemies.IndexOf(E)] = true; // mark this enemy as hit

                                    if (cAction.Num == (int)A.SlideKick || cAction.Num == (int)A.CrouchSlideKick)
                                    {
                                        if (!HoldSlideAttack)  // hold the attack pose
                                        {
                                            HoldSlideAttack = true;
                                            HoldSlideCount = 2;
                                            SlideHold = 15;
                                        }

                                        //cAction = Actions[(int)A.Stand];
                                        Velocity.X = 0;
                                    }

                                    if (cAction.Num == (int)A.WeakJumpKick && Velocity.Y > 0)
                                    {
                                        Velocity.Y = -1.1f; // pop up a little for possible air combos
                                    }
                                }
                            }
                        }

                        else // complex hitbox
                        {
                            if (E.ComplexAttack(this))
                            {
                                int Damage = (int)(BattleCalculator.GetDamage(stats.STR, E.stats.DEF) * (AtkPow[cAction.Num] + ChargePow));

                                Texts.Add(new Text(new Vector2(E.Pos.X - DamageFont.MeasureString(Damage.ToString()).X / 2, E.Pos.Y - E.cAction.Center.Y - 7), new Vector2(0, -.5f), Color.Gray, 60, DamageFont, Damage.ToString()));
                                E.stats.HP -= Damage;

                                if (E.stats.HP > 0)
                                {
                                    E.Effect = SpriteEffectHolder.RedTint;
                                    E.Flash = true;
                                    E.FlashCount = 0;
                                }

                                if (!StartDPS)
                                {
                                    StartDPS = true;
                                }
                                else
                                    TotalDamage += Damage;

                                EnemyCheck[R.Enemies.IndexOf(E)] = true; // mark this enemy as hit

                                if (cAction.Num == (int)A.SlideKick || cAction.Num == (int)A.CrouchSlideKick)
                                {
                                    if (!HoldSlideAttack)  // hold the attack pose
                                    {
                                        HoldSlideAttack = true;
                                        HoldSlideCount = 2;
                                        SlideHold = 15;
                                    }
                                    //cAction = Actions[(int)A.Stand];
                                    Velocity.X = 0;
                                }

                                if (cAction.Num == (int)A.WeakJumpKick && Velocity.Y > 0)
                                {
                                    Velocity.Y = -1.1f; // pop up a little for possible air combos
                                }
                            }
                        }
                    }
                }

                if (E.stats.HP > 0) // don't check collision if enemy is dead or Kic is invincible
                {
                    if (!Invincible)
                    {
                        if (!E.ComplexHitBox) // if the enemy only has the basic single hitbox
                        {
                            if (PRec.Intersects(E.HDRec) && !E.Platform) // their bodies touch and isn't a platform enemy
                            {
                                if (HitDetection.IntersectingPixels(PRec, E.HDRec, E.cAction.Rec[E.cAction.CurrentFrame], E.Texture))
                                    TakeDamage(!FacingRight, R, E.stats.STR, .5f);
                            }
                        }

                        else // complex hitbox
                        {
                            float damage = E.ComplexCollision(this);

                            if (damage > 0)
                            {
                                TakeDamage(!FacingRight, R, E.stats.STR, damage);
                            }
                        }
                    }
                }
            }

            if (HoldSlideAttack && !Invincible) // without checking invincibility Kic might end up being forever invincible if he happens to get hit while hitting an enemy
            {
                if (HoldSlideCount >= 0)
                    HoldSlideCount--;
                else
                {
                    if (cAction.Num == (int)A.SlideKick)
                        cAction = Actions[(int)A.Stand];
                    else
                    {
                        cAction = Actions[(int)A.Crouch];
                        cAction.CurrentFrame = 1;
                        PreviousAction = cAction.Num; // and make it so that the frames arent reset because a new action has started
                    }
                    HoldSlideAttack = false;
                }
            }

            for(int i = 0; i < Texts.Count; i++)
            {
                Texts[i].Life--;

                if (Texts[i].Life <= 0)
                {
                    Texts.Remove(Texts[i]);
                    i--;
                }

                else
                    Texts[i].Pos += Texts[i].Velocity;
            }

            if (Invincible)
            {
                if (InvincibleCount > 0)
                    InvincibleCount--;

                if (InvincibleCount == 0)
                    Invincible = false;
            }

            if (SlideKickDelay > 0)
                SlideKickDelay--;

            if (StartDPS)
            {
                Time++;
                DPS = (float)TotalDamage / ((float)Time / 60f);
            }

            if (SlideHold > 0)
                SlideHold--;

            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
            HDRec = new Rectangle((int)Pos.X + cAction.HDRec.X, (int)Pos.Y + cAction.HDRec.Y, cAction.HDRec.Width, cAction.HDRec.Height);
        }

        public void DisplayStats(SpriteBatch SB, SpriteFont F, Vector2 Cpos)
        {
            int Size = 10;

            SB.DrawString(F, "MaxVelocityX qa   - " + MaxVelocityX.ToString(), new Vector2(Cpos.X, Cpos.Y), Color.Yellow);
            SB.DrawString(F, "MaxVelocityY ws   - " + MaxVelocityY.ToString(), new Vector2(Cpos.X, Cpos.Y + Size), Color.Yellow);
            SB.DrawString(F, "MaxFall ed        - " + MaxFall.ToString(), new Vector2(Cpos.X, Cpos.Y + Size*2), Color.Yellow);
            SB.DrawString(F, "MaxJumpCount rf   - " + MaxJumpCount.ToString(), new Vector2(Cpos.X, Cpos.Y + Size*3), Color.Yellow);
            SB.DrawString(F, "MaxGroundAccel tg - " + MaxGroundAccel.ToString(), new Vector2(Cpos.X, Cpos.Y + Size * 4), Color.Yellow);
            SB.DrawString(F, "MaxAirAccel yh    - " + MaxAirAccel.ToString(), new Vector2(Cpos.X, Cpos.Y + Size * 5), Color.Yellow);
            SB.DrawString(F, "MaxStop uj    - " + MaxStop.ToString(), new Vector2(Cpos.X, Cpos.Y + Size * 6), Color.Yellow);
            SB.DrawString(F, "MaxStopAir ik    - " + MaxStopAir.ToString(), new Vector2(Cpos.X, Cpos.Y + Size * 7), Color.Yellow);
        }

        public void ChangeStats(KeyboardState KB, KeyboardState P)
        {
            if (KB.IsKeyDown(Keys.Q) && P.IsKeyUp(Keys.Q))
                MaxVelocityX += .1f;

            if (KB.IsKeyDown(Keys.A) && P.IsKeyUp(Keys.A))
                MaxVelocityX -= .1f;

            if (KB.IsKeyDown(Keys.W) && P.IsKeyUp(Keys.W))
                MaxVelocityY += .1f;

            if (KB.IsKeyDown(Keys.S) && P.IsKeyUp(Keys.S))
                MaxVelocityY -= .1f;

            if (KB.IsKeyDown(Keys.E) && P.IsKeyUp(Keys.E))
                MaxFall += .01f;

            if (KB.IsKeyDown(Keys.D) && P.IsKeyUp(Keys.D))
                MaxFall -= .01f;

            if (KB.IsKeyDown(Keys.R) && P.IsKeyUp(Keys.R))
                MaxJumpCount += .1f;

            if (KB.IsKeyDown(Keys.F) && P.IsKeyUp(Keys.F))
                MaxJumpCount -= .1f;

            if (KB.IsKeyDown(Keys.T) && P.IsKeyUp(Keys.T))
                MaxGroundAccel += .1f;

            if (KB.IsKeyDown(Keys.G) && P.IsKeyUp(Keys.G))
                MaxGroundAccel -= .1f;

            if (KB.IsKeyDown(Keys.Y) && P.IsKeyUp(Keys.Y))
                MaxAirAccel += .1f;

            if (KB.IsKeyDown(Keys.H) && P.IsKeyUp(Keys.H))
                MaxAirAccel -= .1f;

            if (KB.IsKeyDown(Keys.U) && P.IsKeyUp(Keys.U))
                MaxStop += .1f;

            if (KB.IsKeyDown(Keys.J) && P.IsKeyUp(Keys.J))
                MaxStop -= .1f;

            if (KB.IsKeyDown(Keys.I) && P.IsKeyUp(Keys.I))
                MaxStopAir += .01f;

            if (KB.IsKeyDown(Keys.K) && P.IsKeyUp(Keys.K))
                MaxStopAir -= .01f;
        }

        public void SlowDown(Room R)
        {
            if (!Airborn)
            {
                cAction = Actions[(int)A.Stand];
                if (Velocity.X > .5f)
                    Velocity.X -= MaxStop;
                if (Velocity.X < -.5f)
                    Velocity.X += MaxStop;
                if (Math.Abs(Velocity.X) <= .5f)
                    Velocity.X = 0;
            }

            else
            {
                if (Velocity.X > .2f)
                    Velocity.X -= MaxStopAir;
                if (Velocity.X < -.2f)
                    Velocity.X += MaxStopAir;
                if (Math.Abs(Velocity.X) <= .2f)
                    Velocity.X = 0;
            }

            int Total = 0;

            if (Velocity.X < 0) // Left <--
            {
                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height)
                    {
                        Total = (Rec.X + Rec.Width) - (PRec.X); // how far kic would need to go to hit this tile
                        if (Total > Velocity.X && Total <= 0)  // if Kic's current velocity is more than needed
                        {
                            Pos.X = (Rec.X + Rec.Width) - cAction.PRec.X; // set kic's Pos to a solid int value
                            Velocity.X = 0;
                            break; // already hit a tile, no need to check anymore
                        }
                    }
                }

                if (Velocity.X < 0)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height)
                        {
                            Total = (Rec.X + Rec.Width) - (PRec.X); // how far kic would need to go to hit this tile
                            if (Total > Velocity.X && Total <= 0)  // if Kic's current velocity is more than needed
                            {
                                Pos.X = (Rec.X + Rec.Width) - cAction.PRec.X; // set kic's Pos to a solid int value
                                Velocity.X = 0;
                                break; // already hit a tile, no need to check anymore
                            }
                        }
                    }
                }
            }

            else if(Velocity.X > 0) // Right -->
            {
                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height)
                    {
                        Total = (Rec.X) - (PRec.X + PRec.Width);
                        if (Total < Velocity.X && Total >= 0)
                        {
                            Pos.X = Rec.X + cAction.PRec.X + 1;
                            Velocity.X = 0;
                            break;
                        }
                    }
                }

                if (Velocity.X > 0)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height)
                        {
                            Total = (Rec.X) - (PRec.X + PRec.Width);
                            if (Total < Velocity.X && Total >= 0)
                            {
                                Pos.X = Rec.X + cAction.PRec.X + 1;
                                Velocity.X = 0;
                                break;
                            }
                        }
                    }
                }
            }

            Pos.X += Velocity.X;
            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
        }

        /// <summary>
        /// Moves Kic left or right based on joystick movement. (Direction: false = left, true = right)
        /// Also accounts for Kic being in the air, underwater, or just on the ground.
        /// Backdash movement is handled here too.
        /// </summary>
        public void Move(bool Direction, Room R)
        {
            int Total = 0;

            if (cAction.Num != (int)A.Backdash && cAction.Num != (int)A.SlideKick && cAction.Num != (int)A.CrouchSlideKick && cAction.Num != (int)A.GroundPain && cAction.Num != (int)A.Backflip)
            {
                if (cAction.Num != (int)A.WeakStandKick) // not weak standing kick
                {
                    if(cAction.Num != (int)A.StrongJumpKick && cAction.Num != (int)A.WeakJumpKick) // don't change direction during jump kicks
                        FacingRight = Direction;
                    if (!Airborn)
                    {
                        cAction = Actions[(int)A.Run];
                        if (Direction == false) // Left <--
                        {
                            if (Velocity.X > 0) // if velocity is currently to the right
                                Velocity.X = 0;
                            if (Velocity.X > -MaxVelocityX) // if not at max speed
                                Velocity.X -= MaxGroundAccel;
                            else  // otherwise, at max speed
                                Velocity.X = -MaxVelocityX;
                        }

                        else // Right -->
                        {
                            if (Velocity.X < 0)
                                Velocity.X = 0;
                            if (Velocity.X < MaxVelocityX)
                                Velocity.X += MaxGroundAccel;
                            else
                                Velocity.X = MaxVelocityX;
                        }
                    }

                    else if (Airborn) // Airborn
                    {
                        if (!Direction)
                        {
                            if (Velocity.X > 0)
                                Velocity.X = 0;
                            if (Velocity.X > -MaxVelocityX)
                            {
                                Velocity.X -= MaxAirAccel;
                                if (Velocity.X < -MaxVelocityX)
                                    Velocity.X = -MaxVelocityX;
                            }
                        }

                        else
                        {
                            if (Velocity.X < 0)
                                Velocity.X = 0;
                            if (Velocity.X < MaxVelocityX)
                            {
                                Velocity.X += MaxAirAccel;
                                if (Velocity.X > MaxVelocityX)
                                    Velocity.X = MaxVelocityX;
                            }
                        }
                    }
                }
            }

            else if (cAction.Num == (int)A.Backdash)// backdashing
            {
                DashCount++;
                if (DashCount > 10)
                {
                    cAction = Actions[(int)A.Stand];
                }

                if (FacingRight)
                {
                    Velocity.X = -MaxVelocityX; // move to the left
                }

                else // facing to the left
                {
                    Velocity.X = MaxVelocityX; // so move to the right;
                }
            }

            else if (cAction.Num == (int)A.Backflip)// backflip
            {
                if (DashCount < 10)
                    DashCount++;
                
                else
                    Velocity.X *= .9f;
            }

            else if (cAction.Num == (int)A.SlideKick && !HoldAction)// slidekick
            {
                DashCount++;
                if (DashCount > 12)
                {
                    cAction = Actions[(int)A.Stand];
                }

                if (!HoldSlideAttack) // prevents Kic from moving after an enemy has already been hit, and hold the attack pose for a little bit
                {
                    if (!FacingRight)
                    {
                        Velocity.X = -MaxVelocityX - ((float)Hold / 40); // move to the left
                    }

                    else // facing to the right
                    {
                        Velocity.X = MaxVelocityX + ((float)Hold / 40); // so move to the right;
                    }
                }
            }

            else if (cAction.Num == (int)A.CrouchSlideKick)
            {
                float SlowDown = 1;

                DashCount++;
                if (DashCount > 20)
                {
                    cAction = Actions[(int)A.Crouch];
                    cAction.CurrentFrame = 1; // and also skip the first crouching frame
                    PreviousAction = cAction.Num; // and make it so that the frames arent reset because a new action has started
                }

                if (DashCount > 15)
                    SlowDown = (5 - (DashCount - 16)) / 5f;

                if (SlowDown < 0)
                    SlowDown = 0;

                if (!HoldSlideAttack)
                {
                    if (!FacingRight)
                        Velocity.X = -MaxVelocityX * SlowDown;
                    else
                        Velocity.X = MaxVelocityX * SlowDown;
                }

                else
                    SlowDown += 0;
            }

            else if (HoldAction)
                Velocity.X = 0; // we're holding this action, don't move!

            if (Velocity.X > 0)
            {
                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height)
                    {
                        Total = (Rec.X) - (PRec.X + PRec.Width);
                        if (Total < Velocity.X && Total >= 0)
                        {
                            Pos.X = Rec.X + cAction.PRec.X + 1;
                            Velocity.X = 0;
                            break;
                        }
                    }
                }

                if (Velocity.X != 0)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height)
                        {
                            Total = (Rec.X) - (PRec.X + PRec.Width);
                            if (Total < Velocity.X && Total >= 0)
                            {
                                Pos.X = Rec.X + cAction.PRec.X + 1;
                                Velocity.X = 0;
                                break;
                            }
                        }
                    }
                }
            }

            else if (Velocity.X < 0)
            {
                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height) // if kic is vertically within bounds of this tile
                    {
                        Total = (Rec.X + Rec.Width) - (PRec.X); // how far kic would need to go to hit this tile
                        if (Total > Velocity.X && Total <= 0)  // if Kic's current velocity is more than needed
                        {
                            Pos.X = (Rec.X + Rec.Width) - cAction.PRec.X; // set kic's Pos to a solid int value
                            Velocity.X = 0;
                            break; // already hit a tile, no need to check anymore
                        }
                    }
                }

                if (Velocity.X != 0)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        if (PRec.Y + PRec.Height > Rec.Y && PRec.Y < Rec.Y + Rec.Height) // if kic is vertically within bounds of this tile
                        {
                            Total = (Rec.X + Rec.Width) - (PRec.X); // how far kic would need to go to hit this tile
                            if (Total > Velocity.X && Total <= 0)  // if Kic's current velocity is more than needed
                            {
                                Pos.X = (Rec.X + Rec.Width) - cAction.PRec.X; // set kic's Pos to a solid int value
                                Velocity.X = 0;
                                break; // already hit a tile, no need to check anymore
                            }
                        }
                    }
                }
            }

            //if (Math.Abs(Velocity.X) > MaxVelocityX)
            //{
            //    if (Velocity.X < 0)
            //        Velocity.X = -MaxVelocityX;
            //    else
            //        Velocity.X = MaxVelocityX;
            //}
            
            Pos.X += Velocity.X;
            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);    
        }

        /// <summary>
        /// If Kic isn't standing on anything solid or is jumping, this will make him fall downwards, and check to see if Kic hits any solid ground along the way.
        /// </summary>
        public void Fall(Room R)
        {
            float Total = 0;
            if (cAction.Num != (int)A.WeakJumpKick && cAction.Num != (int)A.StrongJumpKick && cAction.Num != (int)A.GroundPain
                && cAction.Num != (int)A.Backflip) // unless in the middle of a jumpkick, pain, or backflip, set to jumping animation
            {
                cAction = Actions[(int)A.Jump];
                PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
            }

            if (!JumpHold) // If jumping upwards has ended, start accelerating downwards
            {
                Velocity.Y += MaxFall;
                if (Velocity.Y > MaxVelocityY)
                    Velocity.Y = MaxVelocityY;
            }

            if (Velocity.Y > 0 && (PlatformPassWindow <= 0 || (cAction.Num != (int)A.StrongJumpKick && cAction.Num != (int)A.WeakJumpKick && cAction.Num != (int)A.Backflip))) // Down
            { // if just passed through a platform, dont bother colliding with any tiles

                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                    {
                        PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                        Total = (Rec.Y) - (PRec.Y + PRec.Height);
                        if (Total <= Velocity.Y && Total >= 0)
                        {
                            if (cAction.Num != (int)A.StrongJumpKick)
                            {
                                 if (cAction.Num == (int)A.GroundPain)
                                    InvincibleCount = 30; // start Kic's invulnerability
                                cAction = Actions[(int)A.Stand];           
                                Pos.Y = Rec.Y + cAction.PRec.Y;
                            }

                            else
                            {
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                    cAction = Actions[(int)A.Recovery];
                                    Velocity.X = 0;   
                            }
                     
                            Velocity.Y = 0;
                            break;
                        }
                    }
                }

                if (Velocity.Y != 0)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                        {
                            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                            Total = (Rec.Y) - (PRec.Y + PRec.Height);
                            if (Total <= Velocity.Y && Total >= 0)
                            {
                                if (cAction.Num != (int)A.StrongJumpKick)
                                {
                                    if (cAction.Num == (int)A.GroundPain)
                                        InvincibleCount = 30; // start Kic's invulnerability
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                }

                                else
                                {
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                    cAction = Actions[(int)A.Recovery];
                                    Velocity.X = 0;
                                }

                                Velocity.Y = 0;
                                break;
                            }
                        }
                    }
                }

                if (Velocity.Y != 0 && !SkipPlatform)
                {
                    foreach (Rectangle Rec in R.PlatformRecs)
                    {
                        if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                        {
                            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                            Total = (Rec.Y) - (PRec.Y + PRec.Height);
                            if (Total <= Velocity.Y && Total >= 0)
                            {
                                if (cAction.Num != (int)A.StrongJumpKick)
                                {
                                    if (cAction.Num == (int)A.GroundPain)
                                        InvincibleCount = 30; // start Kic's invulnerability
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                }

                                else
                                {
                                    cAction = Actions[(int)A.Stand];
                                    Pos.Y = Rec.Y + cAction.PRec.Y;
                                    cAction = Actions[(int)A.Recovery];
                                    Velocity.X = 0;
                                }

                                Velocity.Y = 0;
                                break;
                            }
                        }
                    }
                }
            }

            if (Velocity.Y < 0) // Up
            {
                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                    {
                        Total = (Rec.Y + Rec.Height) - (PRec.Y); // how far kic would need to go to hit this tile
                        if (Total > Velocity.Y && Total <= 0)  // if Kic's current velocity is more than needed
                        {
                            Pos.Y = (Rec.Y + Rec.Height) - cAction.PRec.Y; // set kic's Pos to a solid int value
                            Velocity.Y = 0;
                            break; // already hit a tile, no need to check anymore
                        }
                    }
                }

                if (Velocity.Y != 0)
                {
                    foreach (Rectangle Rec in R.EnemyRecs)
                    {
                        if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                        {
                            Total = (Rec.Y + Rec.Height) - (PRec.Y); // how far kic would need to go to hit this tile
                            if (Total > Velocity.Y && Total <= 0)  // if Kic's current velocity is more than needed
                            {
                                Pos.Y = (Rec.Y + Rec.Height) - cAction.PRec.Y; // set kic's Pos to a solid int value
                                Velocity.Y = 0;
                                break; // already hit a tile, no need to check anymore
                            }
                        }
                    }
                }
            }

            Pos.Y += Velocity.Y;
            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
        }

        /// <summary>
        /// Keeps track of controls so that if the player stops holding the jump button, Kic will start falling downwards.
        /// If the player doesn't let go of the jump button, Kic will automatically run out of jump time eventually and start falling anyways.
        /// </summary>
        public void Jump(Boolean Start)
        {
            if (Start) // If the jump has just started
            {
                Velocity.Y = -MaxVelocityY;
                JumpCount = (int)MaxJumpCount;
                JumpHold = true;
                cAction = Actions[(int)A.Jump];
            }

            else
            {
                JumpCount--;
                if (JumpCount <= 0)
                    JumpHold = false;
            }
        }

        /// <summary>
        /// Handles Kic's basic weak kicks.  Also checks to see if the kick lands on anything that is kickable (last frame only).
        /// Since it is a weak kick, it can be interrupted at any time by a backdash or jump. (or enemy attacks)
        /// </summary>
        public void WeakKick()
        {
            if (!Airborn)
            {
                if (!Crouching)
                    cAction = Actions[(int)A.WeakStandKick];
                else
                    cAction = Actions[5];     
                Velocity.X = 0; // halt any vertical momentum
            }

            else // is airborn
                cAction = Actions[(int)A.WeakJumpKick];
            
            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
        }

        /// <summary>
        /// Handles Kic's strong kicks.  Also checks to see if the kick lands on anything that is kickable (one frame only).
        /// Strong attacks cannot be interrupted once started. (unless Kic takes any staggering damage)
        /// </summary>
        public void StrongKick()
        {
           
            if (!Airborn)
            {
                if (cAction.Num != (int)A.Crouch)
                    cAction = Actions[(int)A.StrongStandKick];
                else
                    CrouchSlideKick();

                Velocity.X = 0; // halt any vertical momentum
            }

            else
                cAction = Actions[(int)A.StrongJumpKick];


            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
            
        }
        
        /// <summary>
        /// Makes Kic crouch down to the ground.
        /// </summary>
        public void Crouch()
        {
            if (!Airborn)
            {
                cAction = Actions[(int)A.Crouch];
                PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
                Crouching = true;
                Velocity.X = 0;
            }
        }

        /// <summary>
        /// Kic's backdash.  Dash backwards to avoid attacks.  Can be used in the middle of any weak attack.
        /// </summary>
        public void Backdash()
        {
            cAction = Actions[(int)A.Backdash];
            DashCount = 0;
        }

        public void Backflip()
        {
            if(cAction.Num != (int)A.StrongJumpKick)
            {
                cAction = Actions[(int)A.Backflip];
                DashCount = 0;
                if (FacingRight)
                    Velocity.X = -MaxVelocityX;
                else
                    Velocity.X = MaxVelocityX;
                if (Velocity.Y > 0)
                    Velocity.Y = 0;
                JumpHold = false;  // cancel any continued jumping
            }
        }

        /// <summary>
        /// Kic slides forward in kick position, and will stop if an enemy is hit.  The longer it is held, the further and faster it is (to a certain limit).
        /// </summary>
        public void SlideKick()
        {
            if (SlideHold <= 0) // prevents rapid fire slide kicks between hits on enemies
            {
                cAction = Actions[(int)A.SlideKick];
                DashCount = 0;
                HoldSlideAttack = false;
                SlideKickDelay = Actions[(int)A.WeakStandKick].Frames * Actions[(int)A.WeakStandKick].Speed;
            }
        }

        public void CrouchSlideKick()
        {
            if (SlideHold <= 0)
            {
                cAction = Actions[(int)A.CrouchSlideKick];
                HoldSlideAttack = false;
                DashCount = 0;
            }
        }

        public void TakeDamage(Boolean StaggerRight, Room R, int AtkStr, float AtkPow)
        {
            int Total = 0;
            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
            if (cAction.PRec.Y > Actions[(int)A.GroundPain].PRec.Y || cAction.PRec.Bottom < Actions[(int)A.GroundPain].PRec.Bottom)
            {
                foreach (Rectangle Rec in R.TileRecs)
                {
                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                    {
                        Total = (Rec.Y) - (PRec.Y + PRec.Height);
                        if (Total <= Actions[(int)A.GroundPain].PRec.Height - cAction.PRec.Height && Total >= 0)  // if Kic's legs would end up going through any solid ground
                        {
                            Pos.Y = Rec.Y + Actions[(int)A.GroundPain].PRec.Y;
                            break;
                        }

                        if (cAction.PRec.Y > Actions[(int)A.GroundPain].PRec.Y) // if PRec of stagger would end up higher, lower Pos.Y by the difference
                        {
                            if(Actions[(int)A.GroundPain].PRec.Y + Pos.Y < Rec.Bottom && // make sure it only does this for when Kic's rectangle is within the bounds of a tile's rectangle
                               Actions[(int)A.GroundPain].PRec.Y + Pos.Y > Rec.Y)
                            if (Pos.Y + Actions[(int)A.GroundPain].PRec.Y < Rec.Y + Rec.Height)
                            {
                                Pos.Y += cAction.PRec.Y - Actions[(int)A.GroundPain].PRec.Y;
                                break;
                            }
                        }
                    }
                }

                foreach (Rectangle Rec in R.EnemyRecs)
                {
                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                    {
                        Total = (Rec.Y) - (PRec.Y + PRec.Height);
                        if (Total <= Actions[(int)A.GroundPain].PRec.Height - cAction.PRec.Height && Total >= 0)  // if Kic's legs would end up going through any solid ground
                        {
                            Pos.Y = Rec.Y + Actions[(int)A.GroundPain].PRec.Y;
                            break;
                        }

                        if (cAction.PRec.Y > Actions[(int)A.GroundPain].PRec.Y) // if PRec of stagger would end up higher, lower Pos.Y by the difference
                        {
                            if (Actions[(int)A.GroundPain].PRec.Y + Pos.Y < Rec.Bottom && // make sure it only does this for when Kic's rectangle is within the bounds of a tile's rectangle
                               Actions[(int)A.GroundPain].PRec.Y + Pos.Y > Rec.Y)
                                if (Pos.Y + Actions[(int)A.GroundPain].PRec.Y < Rec.Y + Rec.Height)
                                {
                                    Pos.Y += cAction.PRec.Y - Actions[(int)A.GroundPain].PRec.Y;
                                    break;
                                }
                        }
                    }
                }

                foreach (Rectangle Rec in R.PlatformRecs)
                {
                    if (PRec.X + PRec.Width > Rec.X && PRec.X < Rec.X + Rec.Width)
                    {
                        Total = (Rec.Y) - (PRec.Y + PRec.Height);
                        if (Total <= Actions[(int)A.GroundPain].PRec.Height - cAction.PRec.Height && Total >= 0)  // if Kic's legs would end up going through any solid ground
                        {
                            Pos.Y = Rec.Y + Actions[(int)A.GroundPain].PRec.Y;
                            break;
                        }

                        if (cAction.PRec.Y > Actions[(int)A.GroundPain].PRec.Y) // if PRec of stagger would end up higher, lower Pos.Y by the difference
                        {
                            if (Actions[(int)A.GroundPain].PRec.Y + Pos.Y < Rec.Bottom && // make sure it only does this for when Kic's rectangle is within the bounds of a tile's rectangle
                               Actions[(int)A.GroundPain].PRec.Y + Pos.Y > Rec.Y)
                                if (Pos.Y + Actions[(int)A.GroundPain].PRec.Y < Rec.Y + Rec.Height)
                                {
                                    Pos.Y += cAction.PRec.Y - Actions[(int)A.GroundPain].PRec.Y;
                                    break;
                                }
                        }
                    }
                }
            }

            cAction = Actions[(int)A.GroundPain];
            Invincible = true;
            InvincibleCount = -1; // -1 is a flag meaning "Currently staggering, so don't count down for invincibility frames yet"
            if (StaggerRight) // staggering to the right
            {
                Velocity.X = .7f;
                FacingRight = false;
            }

            else
            {
                Velocity.X = -.7f;
                FacingRight = true;
            }

            Velocity.Y = -1.5f;

            PRec = new Rectangle((int)Pos.X + cAction.PRec.X, (int)Pos.Y + cAction.PRec.Y, cAction.PRec.Width, cAction.PRec.Height);
            JumpHold = false; // prevents Kic from going up infinitely if player jumps right before getting hit

            int Damage = AtkStr - stats.DEF;
            Damage = (int)(Damage * AtkPow);
            if(Damage <= 0)
                Damage = 1;

            stats.HP -= Damage;

            Texts.Add(new Text(new Vector2(Pos.X - (DamageFont.MeasureString(Damage.ToString()).X / 2), (PRec.Top - 5) - DamageFont.MeasureString(Damage.ToString()).Y), 
                      new Vector2(0, -.5f), Color.Red, 60, DamageFont, Damage.ToString()));
        }

        public void Animate()
        {
            if (cAction.Num == (int)A.Run)
            {
                if ((cAction.CurrentFrame == 0 || cAction.CurrentFrame == 4) &&
                     cAction.SpeedCount == 1)
                    Step.Play();
            }

            animation.Animate(cAction);
        }

        public Boolean AnimateNL()
        {
            return animation.AnimateNL(cAction);
        }

        public void AnimateHold()
        {
            animation.AnimateHold(cAction);
        }

        public void AddHP(int HP)
        {
            if (stats.HP < stats.MAX_HP)
            {
                stats.HP += HP;
                if(stats.HP > stats.MAX_HP)
                    stats.HP = stats.MAX_HP;
                Texts.Add(new Text(new Vector2(Pos.X - (DamageFont.MeasureString(HP.ToString()).X / 2), (PRec.Top - 5) - DamageFont.MeasureString(HP.ToString()).Y),
                      new Vector2(0, -.5f), new Color(30, 255, 30, 255), 60, DamageFont, HP.ToString()));
            }
        }

        public Rectangle GetFindARec()
        {
            if (FacingRight)
                return FindARec2[cAction.Num][0];
            else
                return FindARec[cAction.Num][0];
        }

        // was trying to fix the camera snapping due to jump kicks, but it didn't quite fix the problem. Maybe if I tweak it better?
        public int AdjustCamera()
        {
            if (cAction.PRec.Bottom != Actions[(int)A.Stand].PRec.Bottom)
                return cAction.PRec.Bottom - Actions[0].PRec.Bottom;

            return 0;
        }

        public void Draw(SpriteBatch spriteBatch, Effect effect, Camera camera)
        {
            if (FacingRight)
                SP = SpriteEffects.FlipHorizontally;        
            else
                SP = SpriteEffects.None;

            if (!Invincible || (Invincible && InvincibleCount % 2 != 0)) // have Kic phase in and out every frame if invincible (or just display normally if not invincible)
            {
                for (int i = 0; i < SCARF_LENGTH; i++)
                {
                    spriteBatch.Draw(KicMove, new Rectangle((int)Scarf[i].X, (int)Scarf[i].Y, 3, 3), new Rectangle(39, 0, 1, 1), Color.White);
                }

                if (Flash)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, effect, camera.GetViewMatrix(Vector2.One));
                    FlashCount++;
                }

                spriteBatch.Draw(KicMove, new Vector2((int)Pos.X + ShakePos, (int)Pos.Y), cAction.Rec[cAction.CurrentFrame], Color.White, Rotation, cAction.Center, 1, SP, 0);

                if (Flash)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, camera.GetViewMatrix(Vector2.One));
                    if (FlashCount > 3)
                        Flash = false;
                }
            }

            //spriteBatch.DrawString(DamageFont, cAction.Num.ToString(), new Vector2(Pos.X, Pos.Y - 30), Color.White);
            //spriteBatch.Draw(KicMove, PRec, new Rectangle(39, 0, 1, 1), new Color(0, 255, 0, 50));

            //if(Test != null)
                //spriteBatch.Draw(Test, new Vector2(Pos.X, Pos.Y - 50), Color.White);

            foreach (Text T in Texts)
            {
                T.Display(spriteBatch);
            }

            //spriteBatch.DrawString(DamageFont, DPS.ToString(), new Vector2(Pos.X, Pos.Y - 30), Color.Blue);
        }
    }
}
