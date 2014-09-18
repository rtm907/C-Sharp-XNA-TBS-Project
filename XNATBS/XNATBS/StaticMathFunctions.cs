using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
//using System.Drawing;

namespace XNATBS
{
    static class StaticMathFunctions
    {
        public static Direction OppositeDirection(Direction d)
        {
            return (Direction)(((byte)d + 3) % 6);
        }

        public static Direction DirectionToTheRight(Direction d)
        {
            return (Direction)(((byte)d + 1) % 6);
        }

        public static Direction DirectionToTheLeft(Direction d)
        {
            return (Direction)(((byte)d + 5) % 6);
        }

        public static Coords CoordsAverage(Coords c1, Coords c2)
        {
            return new Coords(c1.Type, (Int32)0.5 * (c1.X + c2.X), (Int32)0.5 * (c1.Y + c2.Y));
        }

        public static bool CoordinateIsInBox(Coords c, Coords boxTopLeft, Coords boxBottomRight)
        {
            return (((c.X >= boxTopLeft.X) && (c.X <= boxBottomRight.X)) && ((c.Y >= boxTopLeft.Y) && (c.Y <= boxBottomRight.Y)));
        }

        /// <summary>
        /// Returns the eucledean distance between two Coords
        /// </summary>
        public static float DistanceBetweenTwoCoordsEucledean(Coords c1, Coords c2)
        {
            return (float)Math.Sqrt(Math.Pow((c1.X - c2.X), 2) + Math.Pow((c1.Y - c2.Y), 2));
        }

        /// <summary>
        /// returns the distance between two Coords
        /// </summary>
        public static float DistanceBetweenTwoCoords(Coords c1, Coords c2)
        {
            return Math.Max(Math.Abs(c1.X - c2.X), Math.Abs(c1.Y - c2.Y));
        }
    
        // Helper functions for the Hex-coords to Array-coords transforms; see reference below.
        private static Int32 Floor2 (Int32 X) {return ((X) >= 0) ? (X>>1) : (((X)-1)/2);}        
        private static Int32 Ceil2 (Int32 X) {return ((X) >= 0) ? (((X)+1)>>1) : ((X)/2);}


        /// <summary>
        /// returns the distance between two Coords in hexes
        /// </summary>
        public static Int32 DistanceBetweenTwoCoordsHex(Coords c1, Coords c2)
        {
            // reference: http://www-cs-students.stanford.edu/~amitp/Articles/HexLOS.html

            Coords A = new Coords();
            Coords B = new Coords();

            A.X = c1.X - Floor2(c1.Y);
            A.Y = c1.X + Ceil2(c1.Y);
            B.X = c2.X - Floor2(c2.Y);
            B.Y = c2.X + Ceil2(c2.Y);
            // calculate distance using hexcoords as per previous algorithm
            Int32 dx = B.X - A.X;
            Int32 dy = B.Y - A.Y;

            return (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dx - dy)) / 2;
        }

        public static Int32 DistanceBetweenTwoCoordsEucledeanSquared(Coords c1, Coords c2)
        {
            Int32 dx = c1.X - c2.X;
            Int32 dy = c1.Y - c2.Y;
            return (dx * dx + dy * dy);
        }

        /* BROKEN
        /// <summary>
        /// Returns the Direction in which a vector is pointing.
        /// </summary>
        public static Nullable<Direction> DirectionVectorToDirection(Coords dirvector)
        {
            if (dirvector.X == 0 & dirvector.Y == 0)
            {
                return null;
            }

            // The angle is clockwise from the negative X, Y=0 axis. Note the positive Y-axis points down.
            double angle;
            angle = Math.Atan2(dirvector.Y, dirvector.X+0.5*(Math.Abs(dirvector.Y)%2)) + Math.PI;

            Direction moveDir = (Direction)
               (sbyte)((((angle) / (Math.PI / 3)) + 4) % 6);

            return moveDir;
        }
        */

        public static float InfluenceDecayFunction1(UInt32 a)
        {
            return (float)1 / (a + 1);
        }

        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.X, -vector.Y);
        }


        /// <summary>
        /// Returns the Coords that neighbour 'here' in 'direction'.
        /// Note C# forms coordinate system has origin at the top-left
        /// </summary>
        public static Coords CoordsNeighboringInDirection(Coords here, Direction direction)
        {
            switch (direction)
            {
                case (Direction.Northeast):
                    return new Coords(here.Type, here.X + (here.Y % 2), here.Y - 1);
                case (Direction.East):
                    return new Coords(here.Type, here.X + 1, here.Y);
                case (Direction.Southeast):
                    return new Coords(here.Type, here.X + (here.Y % 2), here.Y + 1);
                case (Direction.Southwest):
                    return new Coords(here.Type, here.X - ((here.Y + 1) % 2), here.Y + 1);
                case (Direction.West):
                    return new Coords(here.Type, here.X - 1, here.Y);
                case (Direction.Northwest):
                    return new Coords(here.Type, here.X - ((here.Y + 1) % 2), here.Y - 1);
            }

            // This code should be unreachable. Added because compiler wants it.
            return here;
        }

        public static UInt16 StandardFormulaHP(UInt16 strength)
        {
            return (UInt16)(strength * 4);
        }

        public static UInt16 StandardFormulaAP(UInt16 agility)
        {
            return (UInt16)(agility * 2);
        }

        /// <summary>
        /// Returns the experience gained upon defeating the passed Creature parameter.
        /// Currently the XP gain is the sum of the target's main stats to the power of 3/2 (to reward defeating tougher enemies).
        /// </summary>
        public static UInt16 XPFormula(Creature cible)
        {
            UInt16 returnValue = 0;
            for (int i = 0; i < (sbyte)Creature.StatMain.COUNT; ++i)
            {
                returnValue += cible.GetStatMain((Creature.StatMain)i, false);
            }

            return (UInt16) Math.Pow(returnValue,1.5);
        }

        /*
        // Returns the coordinate-wise representation of a Direction
        public static Coords DirectionToCoords(Direction dir)
        {
            switch (dir)
            {
                case (Direction.Northeast):
                    return new Coords(1, -1);
                case (Direction.East):
                    return new Coords(1, 0);
                case (Direction.Southeast):
                    return new Coords(1, 1);
                case (Direction.Southwest):
                    return new Coords(-1, 1);
                case (Direction.West):
                    return new Coords(-1, 0);
                case (Direction.Northwest):
                    return new Coords(-1, -1);
            }

            return new Coords(CoordsType.Pixel, 0, 0);
        }
        */

        /*
        /// <summary>
        /// Evaluation function for the AI decision making algorithm.
        /// </summary>
        public static float StimulusEvaluator(float strength, float distance)
        {
            // expensive function; consider simplyfying
            return strength / (1 + Constants.StimulusEvaluationDistanceRedundancyCoefficient *
                (float)Math.Pow(distance, Constants.StimulusEvaluationDistanceRedundancyPower));
        }
        */
    }
}
