using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KIC
{
    abstract class Enemy // make this abstract so that it's impossible to make just an "Enemy" without a derived class
    {
        public Vector2 Pos;
        public Action cAction;
        public List<Action> Actions;
        public Animation animation;
        public Texture2D Texture;
        public SpriteEffects SE;
        public int ID;  // Identification number, to identify each enemy type
        public Boolean Directional;
        public Rectangle HDRec;
        public Stats stats;
        public Boolean Alive;
        public Boolean Platform;
        public EventHandler Killed;
        public Effect Effect;
        public Boolean Flash;
        public int FlashCount;
        public int[] EnergyDrop;
        public Boolean LootDropped;
        public Boolean ExplosionSize;
        public Boolean RapidExplosions;
        public Boolean ComplexHitBox;
        public bool Attacking;

        public Enemy()
        {
            this.Actions = new List<Action>();
            this.animation = new Animation();
            this.stats = new Stats();
            this.Alive = true;
            this.Platform = false;
            this.LootDropped = false;
            ComplexHitBox = false; // by default this is false, unless otherwise specified in an enemie's initialization.
            Attacking = false;
        }

        public virtual void SetTileNum(int Num) { }

        public virtual void SetRandom(Random R) { }

        public virtual void SetByte(Boolean FacingRight) { }

        public virtual float ComplexCollision(KIC_Object Kic) { return 0; }

        public virtual bool ComplexAttack(KIC_Object Kic) { return false; }

        public virtual void Update(Room room, KIC_Object Kic) { }

        public virtual void Draw(SpriteBatch spriteBatch) { }

        public virtual void Draw(SpriteBatch spriteBatch, Color C) { }
    }
}