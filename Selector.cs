using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	class Selector
	{
		private Keys increment, decrement;
		private int current;
		private int length;
		private int lastFrameIndex;
		private string[] names;

		public Selector(int length, bool horizontal = true, string[] names = null)
		{
			this.length = length;
			if(horizontal)
			{
				increment = Keys.Right;
				decrement = Keys.Left;
			}
			else
			{
				increment = Keys.Down;
				decrement = Keys.Up;
			}
			current = 0;

			lastFrameIndex = 0;

			if (names == null)
			{
				names = new string[length];
				for (int i = 0; i < names.Length; i++)
					names[i] = "";
			}
			this.names = names;
		}

		public bool Update(KeyboardState prevState)
		{
			lastFrameIndex = current;

			if(Keyboard.GetState().IsKeyDown(decrement) && prevState.IsKeyUp(decrement))
			{
				if (current-- == 0)
					current = length-1;
				return true;
			}
			else if (Keyboard.GetState().IsKeyDown(increment) && prevState.IsKeyUp(increment))
			{
				if (++current == length)
					current = 0;
				return true;
			}
			return false;
		}

		public int GetIndex()
		{
			return current;
		}

		public void SetIndex(int index)
		{
			if (index < length && index >= 0)
				current = index;
		}

		public bool IndexChanged()
		{
			return (lastFrameIndex != current);
		}

		public string GetName()
		{
			return names[current];
		}
	}
}
