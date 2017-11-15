using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

/// <summary>
/// This class handles any hit detection between 2 sprites.
/// Since some sprites are multidirectional, there are different overloads for IntersectingPixels
/// </summary>
public static class HitDetection
{
    static HitDetection()
    {

    }

    /// <summary>
    /// Determines if there is overlap of the non-transparent pixels between two sprites.
    /// </summary>
    /// <param name="rectangleA">Bounding rectangle of the first sprite</param>
    /// <param name="rectangleB">Bounding rectangle of the second sprite</param>
    /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
    static public bool IntersectingPixels(Rectangle rectangleA, Rectangle sourceA, Texture2D TA, Boolean FacingRight, int Center,
                                          Rectangle rectangleB, Rectangle sourceB, Texture2D TB)
    {
        // Find the bounds of the rectangle intersection
        int top = Math.Max(rectangleA.Top, rectangleB.Top);
        int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
        int left = Math.Max(rectangleA.Left, rectangleB.Left);
        int right = Math.Min(rectangleA.Right, rectangleB.Right);

        Rectangle Intersection = new Rectangle(left, top, right - left, bottom - top); // the rectangle of intersection
        Rectangle SegmentA;
        Rectangle SegmentB;
        Rectangle SegmentATest;

        // the source rectangle on the sprite sheet where the overlapping of rectangles happens
        SegmentATest = new Rectangle(sourceA.Left + (Intersection.Left - rectangleA.Left), sourceA.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        //SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);
        SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);
        int YDif = sourceA.Height - rectangleA.Height;
        SegmentA = new Rectangle((sourceA.Left + Center) + (rectangleA.Left - Intersection.Left) + 1, sourceA.Top + (Intersection.Top - rectangleA.Top) + YDif, Intersection.Width, Intersection.Height);
        //SegmentA = new Rectangle(sourceA.Left + (Intersection.Left - rectangleA.Left), sourceA.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);

        /*if (FacingRight)
        { // "mirror" the location of the segment rectangle since the sprite is flipped when facing right
            //SegmentA.X -= Center - ((sourceA.X + Center) - SegmentA.X);
            SegmentA.X = ((sourceA.Left + Center) * 2) - (SegmentA.Left + SegmentA.Width) - 1;  // *** trying to figure out how to mirror the SegmentA based on Center...
            SegmentATest.X = ((sourceA.Left + Center) * 2) - (SegmentATest.Left + SegmentATest.Width) - 1;
            //SegmentA.X = (sourceA.Left + Center) - (SegmentA.Right - Center);
        }*/

        // ok so....with how SegmentA is determined it somehow doesn't need to be flipped around the center if Kic is facing right.

        Color[] bitsA = new Color[Intersection.Width * Intersection.Height];
        TA.GetData<Color>(0, SegmentA, bitsA, 0, bitsA.Length);

        Color[] temp = new Color[SegmentA.Width];

        if (FacingRight)
        { // flip the data horizontally, so that it's facing right
            for (int y = 0; y < SegmentA.Height; y++) // loop through each row
            {
                int i = 0;

                for (int x = y * SegmentA.Width; x < (y * SegmentA.Width) + SegmentA.Width; x++) // store this row in a temporary array
                {
                    temp[i] = bitsA[x];
                    i++;
                }

                i = 0;

                temp.Reverse(); // switch it around

                for (int x = y * SegmentA.Width; x < (y * SegmentA.Width) + SegmentA.Width; x++) // store this row in a temporary array
                {
                    bitsA[x] = temp[i];
                    i++;
                }
            }
        }

        //Texture2D Test2 = new Texture2D(TA.GraphicsDevice, SegmentA.Width, SegmentB.Height);
        //Test2.SetData(bitsA);

        Color[] bitsB = new Color[Intersection.Width * Intersection.Height];
        TB.GetData<Color>(0, SegmentB, bitsB, 0, bitsB.Length);

        for (int i = 0; i < bitsA.Length; i++)
        {
            Color colorA = bitsA[i];
            Color colorB = bitsB[i];

             //If both pixels are not completely transparent,
            if (colorA.A != 0 && colorB.A != 0)    
                return true; // then an intersection has been found
        }

        // No intersection found
        return false;
    }

    /// <summary>
    /// This is used for attack hit detection
    /// between two sprites.
    /// </summary>
    /// <param name="rectangleA">Bounding rectangle of the first sprite</param>
    /// <param name="rectangleB">Bounding rectangle of the second sprite</param>
    /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
    static public bool IntersectingPixels(Rectangle rectangleA, Rectangle sourceA, Texture2D TA, Boolean FacingRight,
                                          Rectangle rectangleB, Rectangle sourceB, Texture2D TB)
    {
        // Find the bounds of the rectangle intersection
        int top = Math.Max(rectangleA.Top, rectangleB.Top);
        int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
        int left = Math.Max(rectangleA.Left, rectangleB.Left);
        int right = Math.Min(rectangleA.Right, rectangleB.Right);

        Rectangle Intersection = new Rectangle(left, top, right - left, bottom - top); // the rectangle of intersection
        Rectangle SegmentA;
        Rectangle SegmentB;

        // the source rectangle on the sprite sheet where the overlapping of rectangles happens
        SegmentA = new Rectangle(sourceA.Left + (Intersection.Left - rectangleA.Left), sourceA.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);

        Color[] bitsA = new Color[Intersection.Width * Intersection.Height];
        TA.GetData<Color>(0, SegmentA, bitsA, 0, bitsA.Length);

        Color[] temp = new Color[SegmentA.Width];

        if (FacingRight)
        { // flip the data horizontally, so that it's facing right
            for (int y = 0; y < SegmentA.Height; y++) // loop through each row
            {
                int i = 0;

                for (int x = y * SegmentA.Width; x < (y * SegmentA.Width) + SegmentA.Width; x++) // store this row in a temporary array
                {
                    temp[i] = bitsA[x];
                    i++;
                }

                i = 0;

                temp.Reverse(); // switch it around

                for (int x = y * SegmentA.Width; x < (y * SegmentA.Width) + SegmentA.Width; x++) // store this row in a temporary array
                {
                    bitsA[x] = temp[i];
                    i++;
                }
            }
        }

        //Texture2D Test2 = new Texture2D(TA.GraphicsDevice, SegmentA.Width, SegmentB.Height);
        //Test2.SetData(bitsA);

        Color[] bitsB = new Color[Intersection.Width * Intersection.Height];
        TB.GetData<Color>(0, SegmentB, bitsB, 0, bitsB.Length);

        for (int i = 0; i < bitsA.Length; i++)
        {
            Color colorA = bitsA[i];
            Color colorB = bitsB[i];

            //If both pixels are not completely transparent,
            if (colorA.A != 0 && colorB.A != 0)
                return true; // then an intersection has been found
        }

        // No intersection found
        return false;
    }

    /// <summary>
    /// This is when object A has a specific attack rectangle and is directional, and object B just uses a standard HDRec that covers the entire sprite and is directional
    /// </summary>
    /// <param name="rectangleA"></param>
    /// <param name="sourceA"></param>
    /// <param name="centerA"></param>
    /// <param name="attackRecA"></param>
    /// <param name="TA"></param>
    /// <param name="FacingRightA"></param>
    /// <param name="rectangleB"></param>
    /// <param name="sourceB"></param>
    /// <param name="TB"></param>
    /// <param name="FacingRightB"></param>
    /// <returns></returns>
    static public bool IntersectingPixels(Rectangle rectangleA, Rectangle sourceA, Vector2 centerA, Rectangle attackRecA, Texture2D TA, Boolean FacingRightA,
                                          Rectangle rectangleB, Rectangle sourceB, Texture2D TB, Boolean FacingRightB)
    {
        // Find the bounds of the rectangle intersection
        int top = Math.Max(rectangleA.Top, rectangleB.Top);
        int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
        int left = Math.Max(rectangleA.Left, rectangleB.Left);
        int right = Math.Min(rectangleA.Right, rectangleB.Right);

        Rectangle Intersection = new Rectangle(left, top, right - left, bottom - top); // the rectangle of intersection
        Rectangle SegmentA;
        Rectangle SegmentB;

        Rectangle actualSegment = new Rectangle(sourceA.X + (int)centerA.X + attackRecA.X, sourceA.Y + (int)centerA.Y + attackRecA.Y, attackRecA.Width, attackRecA.Height);

        // the source rectangle on the sprite sheet where the overlapping of rectangles happens
        //SegmentA = new Rectangle(sourceA.Left + (Intersection.Left - rectangleA.Left), sourceA.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentA = new Rectangle(actualSegment.Left + (Intersection.Left - rectangleA.Left), actualSegment.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);

        Color[] bitsA = new Color[sourceA.Width * sourceA.Height];
        Color[] flipA = new Color[bitsA.Length];
        TA.GetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);

        if (FacingRightA)
        {
            for (int x = 0; x < sourceA.Width; x++)
            {
                for (int y = 0; y < sourceA.Height; y++)
                {
                    int index = 0;

                    index = sourceA.Width - 1 - x + y * sourceA.Width;

                    flipA[x + y * sourceA.Width] = bitsA[index];
                }
            }

            TA.SetData<Color>(0, sourceA, flipA, 0, flipA.Length);
        }

        Color[] bitsB = new Color[sourceB.Width * sourceB.Height];
        TB.GetData<Color>(0, sourceB, bitsB, 0, bitsB.Length);
        Color[] flipB = new Color[bitsB.Length];

        if (FacingRightB)
        {
            for (int x = 0; x < sourceB.Width; x++)
            {
                for (int y = 0; y < sourceB.Height; y++)
                {
                    int index = 0;

                    index = sourceB.Width - 1 - x + y * sourceB.Width;

                    flipB[x + y * sourceB.Width] = bitsB[index];
                }
            }

            TB.SetData<Color>(0, sourceB, flipB, 0, flipB.Length);
        }

        Color[] CheckA = new Color[SegmentA.Width * SegmentA.Height];
        TA.GetData<Color>(0, SegmentA, CheckA, 0, CheckA.Length);
        Color[] CheckB = new Color[SegmentB.Width * SegmentB.Height];
        TB.GetData<Color>(0, SegmentB, CheckB, 0, CheckB.Length);

        for (int i = 0; i < CheckA.Length; i++)
        {
            Color colorA = CheckA[i];
            Color colorB = CheckB[i];

            //If both pixels are not completely transparent,
            if (colorA.A != 0 && colorB.A != 0)
            {
                if (FacingRightB) // flip any flipped parts on the spritesheet
                    TB.SetData<Color>(0, sourceB, bitsB, 0, bitsB.Length);
                if (FacingRightA)
                    TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);
                return true; // then an intersection has been found
            }
        }

        if (FacingRightB)
            TB.SetData<Color>(0, sourceB, bitsB, 0, bitsB.Length);
        if (FacingRightA)
            TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);

        return false;
    }


    /// <summary>
    /// For when A has a specific attack rectangle and is directional, and B has a standard HDRec and is non-directional
    /// </summary>
    static public bool IntersectingPixels(Rectangle rectangleA, Rectangle sourceA, Vector2 centerA, Rectangle attackRecA, Texture2D TA, Boolean FacingRightA,
                                          Rectangle rectangleB, Rectangle sourceB, Texture2D TB)
    {
        // Find the bounds of the rectangle intersection
        int top = Math.Max(rectangleA.Top, rectangleB.Top);
        int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
        int left = Math.Max(rectangleA.Left, rectangleB.Left);
        int right = Math.Min(rectangleA.Right, rectangleB.Right);

        Rectangle Intersection = new Rectangle(left, top, right - left, bottom - top); // the rectangle of intersection
        Rectangle SegmentA;
        Rectangle SegmentB;

        Rectangle actualSegment = new Rectangle(sourceA.X + (int)centerA.X + attackRecA.X, sourceA.Y + (int)centerA.Y + attackRecA.Y, attackRecA.Width, attackRecA.Height);

        // the source rectangle on the sprite sheet where the overlapping of rectangles happens
        //SegmentA = new Rectangle(sourceA.Left + (Intersection.Left - rectangleA.Left), sourceA.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentA = new Rectangle(actualSegment.Left + (Intersection.Left - rectangleA.Left), actualSegment.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);

        Color[] bitsA = new Color[sourceA.Width * sourceA.Height];
        Color[] flipA = new Color[bitsA.Length];
        TA.GetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);

        if (FacingRightA)
        {
            for (int x = 0; x < sourceA.Width; x++)
            {
                for (int y = 0; y < sourceA.Height; y++)
                {
                    int index = 0;

                    index = sourceA.Width - 1 - x + y * sourceA.Width;

                    flipA[x + y * sourceA.Width] = bitsA[index];
                }
            }

            TA.SetData<Color>(0, sourceA, flipA, 0, flipA.Length);
        }

        Color[] bitsB = new Color[sourceB.Width * sourceB.Height];
        TB.GetData<Color>(0, sourceB, bitsB, 0, bitsB.Length);

        Color[] CheckA = new Color[SegmentA.Width * SegmentA.Height];
        TA.GetData<Color>(0, SegmentA, CheckA, 0, CheckA.Length);
        Color[] CheckB = new Color[SegmentB.Width * SegmentB.Height];
        TB.GetData<Color>(0, SegmentB, CheckB, 0, CheckB.Length);

        for (int i = 0; i < CheckA.Length; i++)
        {
            Color colorA = CheckA[i];
            Color colorB = CheckB[i];

            //If both pixels are not completely transparent,
            if (colorA.A != 0 && colorB.A != 0)
            {
                if (FacingRightA)
                    TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);
                return true; // then an intersection has been found
            }
        }

        if (FacingRightA)
            TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);

        return false;
    }

    /// <summary>
    /// This is for when one of the sprites has a "hit zone" that isn't actually determined by non-transparent pixels
    /// (for example, with Kic's running animation the area between his legs can still be hit)
    /// </summary>
    /// <param name="rectangleA"></param>
    /// <param name="rectangleB"></param>
    /// <param name="sourceB"></param>
    /// <param name="TB"></param>
    /// <returns></returns>
    static public bool IntersectingPixels(Rectangle rectangleA,
                                          Rectangle rectangleB, Rectangle sourceB, Texture2D TB)
    {
        // Find the bounds of the rectangle intersection
        int top = Math.Max(rectangleA.Top, rectangleB.Top);
        int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
        int left = Math.Max(rectangleA.Left, rectangleB.Left);
        int right = Math.Min(rectangleA.Right, rectangleB.Right);

        Rectangle Intersection = new Rectangle(left, top, right - left, bottom - top); // the rectangle of intersection
        Rectangle SegmentB;

        // the source rectangle on the sprite sheet where the overlapping of rectangles happens
        SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);

        Color[] bitsB = new Color[Intersection.Width * Intersection.Height];
        TB.GetData<Color>(0, SegmentB, bitsB, 0, bitsB.Length);

        for (int i = 0; i < bitsB.Length; i++)
        {
            Color colorB = bitsB[i];

            //If there is any non-transparent pixels
            if (colorB.A != 0)
                return true; // then an intersection has been found
        }

        // No intersection found
        return false;
    }

    /// <summary>
    /// This is used for testing purposes, it allows me to see on-screen which segment of a sprite is being used for hit detection
    /// </summary>
    /// <param name="rectangleA"></param>
    /// <param name="sourceA"></param>
    /// <param name="centerA"></param>
    /// <param name="attackRecA"></param>
    /// <param name="TA"></param>
    /// <param name="FacingRightA"></param>
    /// <param name="rectangleB"></param>
    /// <param name="sourceB"></param>
    /// <param name="TB"></param>
    /// <param name="FacingRightB"></param>
    /// <returns>A texture of just the segment being checked for hit detection, which I can then display on-screen</returns>
    static public Texture2D CheckSegment(Rectangle rectangleA, Rectangle sourceA, Vector2 centerA, Rectangle attackRecA, Texture2D TA, Boolean FacingRightA,
                                          Rectangle rectangleB, Rectangle sourceB, Texture2D TB, Boolean FacingRightB)
    {
        // Find the bounds of the rectangle intersection
        int top = Math.Max(rectangleA.Top, rectangleB.Top);
        int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
        int left = Math.Max(rectangleA.Left, rectangleB.Left);
        int right = Math.Min(rectangleA.Right, rectangleB.Right);

        Rectangle Intersection = new Rectangle(left, top, right - left, bottom - top); // the rectangle of intersection
        Rectangle SegmentA;
        Rectangle SegmentB;

        Rectangle actualSegment = new Rectangle(sourceA.X + (int)centerA.X + attackRecA.X, sourceA.Y + (int)centerA.Y + attackRecA.Y, attackRecA.Width, attackRecA.Height);

        // the source rectangle on the sprite sheet where the overlapping of rectangles happens
        //SegmentA = new Rectangle(sourceA.Left + (Intersection.Left - rectangleA.Left), sourceA.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentA = new Rectangle(actualSegment.Left + (Intersection.Left - rectangleA.Left), actualSegment.Top + (Intersection.Top - rectangleA.Top), Intersection.Width, Intersection.Height);
        SegmentB = new Rectangle(sourceB.Left + (Intersection.Left - rectangleB.Left), sourceB.Top + (Intersection.Top - rectangleB.Top), Intersection.Width, Intersection.Height);

        Color[] bitsA = new Color[sourceA.Width * sourceA.Height];
        Color[] flipA = new Color[bitsA.Length];
        TA.GetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);

        if (FacingRightA)
        {
            for (int x = 0; x < sourceA.Width; x++)
            {
                for (int y = 0; y < sourceA.Height; y++)
                {
                    int index = 0;

                    index = sourceA.Width - 1 - x + y * sourceA.Width;

                    flipA[x + y * sourceA.Width] = bitsA[index];
                }
            }

            TA.SetData<Color>(0, sourceA, flipA, 0, flipA.Length);
            //TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);
        }

        Texture2D Test = new Texture2D(Graphics.GetGraphicsDevice(), sourceA.Width, sourceA.Height);
        Test.SetData<Color>(flipA);
        TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);
        return Test;


        Color[] bitsB = new Color[sourceB.Width * sourceB.Height];
        TB.GetData<Color>(0, sourceB, bitsB, 0, bitsB.Length);
        Color[] flipB = new Color[bitsB.Length];

        if (FacingRightB)
        {
            for (int x = 0; x < sourceB.Width; x++)
            {
                for (int y = 0; y < sourceB.Height; y++)
                {
                    int index = 0;

                    index = sourceB.Width - 1 - x + y * sourceB.Width;

                    flipB[x + y * sourceB.Width] = bitsB[index];
                }
            }

            TB.SetData<Color>(0, sourceB, flipB, 0, flipB.Length);
        }

        Color[] CheckA = new Color[SegmentA.Width * SegmentA.Height];
        TA.GetData<Color>(0, SegmentA, CheckA, 0, CheckA.Length);
        Color[] CheckB = new Color[SegmentB.Width * SegmentB.Height];
        TB.GetData<Color>(0, SegmentB, CheckB, 0, CheckB.Length);

        //Texture2D Test = new Texture2D(Graphics.GetGraphicsDevice(), sourceB.Width, sourceB.Height);
        if (FacingRightB)
            TB.SetData<Color>(0, sourceB, bitsB, 0, bitsB.Length);
        if (FacingRightA)
            TA.SetData<Color>(0, sourceA, bitsA, 0, bitsA.Length);
        //Test.SetData<Color>(bitsB);

        return Test;

        //Texture2D Test = new Texture2D(Graphics.GetGraphicsDevice(), SegmentB.Width, SegmentB.Height);
        //Texture2D Test = new Texture2D(Graphics.GetGraphicsDevice(), sourceB.Width, sourceB.Height);
        //Test.SetData(bitsB);

        return null;
    }
}

