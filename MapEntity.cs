using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	abstract class MapEntity
	{
		public Body body;

		public static bool operator < (MapEntity lhs, MapEntity rhs)
		{
			return lhs.body.Position.Y < rhs.body.Position.Y;
		}

		public static bool operator >(MapEntity lhs, MapEntity rhs)
		{
			return lhs.body.Position.Y > rhs.body.Position.Y;
		}

		public abstract void Draw(SpriteBatch pSb);
	}
}
