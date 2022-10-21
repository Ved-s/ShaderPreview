using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.Structures;
using System.Collections;
using System.Collections.Generic;

namespace ShaderPreview.UI.Elements
{
    public class UIContainer : UIElement
    {
        public ElementCollection Elements { get; protected set; }

        public Offset4 Padding;

        private List<UIElement> UpdateList = new();

        public override UIRoot Root
        {
            get => base.Root;
            protected internal set
            {
                base.Root = value;
                foreach (UIElement element in Elements)
                    element.Root = value;
            }
        }

        protected bool SkipRecalculatingChildren = false;

        public UIContainer()
        {
            Elements = new(this);
        }

        protected override void UpdateSelf()
        {
            PreUpdateChildren();

            UpdateList.Clear();
            UpdateList.AddRange(Elements);

            foreach (UIElement element in UpdateList)
                if (element.Enabled && element.Visible)
                    element.Update();
            PostUpdateChildren();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            PreDrawChildren(spriteBatch);
            foreach (UIElement element in Elements)
                if (element.Visible)
                    element.Draw(spriteBatch);
            PostDrawChildren(spriteBatch);
        }

        protected virtual void PreUpdateChildren() { }
        protected virtual void PostUpdateChildren() { }

        protected virtual void PreDrawChildren(SpriteBatch spriteBatch) { }
        protected virtual void PostDrawChildren(SpriteBatch spriteBatch) { }

        protected virtual void ChildHoveredChanged() { }

        public override void Recalculate()
        {
            base.Recalculate();
            if (!SkipRecalculatingChildren)
                foreach (UIElement element in Elements)
                    if (element.Visible && element.Enabled)
                        element.Recalculate();
            SkipRecalculatingChildren = false;
        }

        public UIElement? GetElementAt(Vec2 screenpos, bool canReturnSelf)
        {
            if (!ScreenRect.Contains(screenpos))
                return null;

            foreach (UIElement element in Elements)
            {
                if (!element.Visible || !element.ScreenRect.Contains(screenpos))
                    continue;

                if (element is not UIContainer container)
                    return element;

                return container.GetElementAt(screenpos, true);
            }

            return canReturnSelf ? this : null;
        }

        public class ElementCollection : IList<UIElement>
        {
            private readonly UIContainer Parent;
            private List<UIElement> Elements = new();

            public UIElement this[int index]
            {
                get => Elements[index];
                set
                {
                    ResetParent(Elements[index]);
                    Elements[index] = value;
                    SetParent(value);
                }
            }

            public int Count => Elements.Count;

            public bool IsReadOnly => false;

            public ElementCollection(UIContainer parent)
            {
                Parent = parent;
            }

            public void Add(UIElement item)
            {
                Elements.Add(item);
                SetParent(item);
            }

            public void Clear()
            {
                foreach (UIElement element in Elements)
                    ResetParent(element);

                Elements.Clear();
            }

            public bool Contains(UIElement item)
            {
                return Elements.Contains(item);
            }

            public void CopyTo(UIElement[] array, int arrayIndex)
            {
                Elements.CopyTo(array, arrayIndex);
            }

            public IEnumerator<UIElement> GetEnumerator()
            {
                return Elements.GetEnumerator();
            }

            public int IndexOf(UIElement item)
            {
                return Elements.IndexOf(item);
            }

            public void Insert(int index, UIElement item)
            {
                Elements.Insert(index, item);
                SetParent(item);
            }

            public bool Remove(UIElement item)
            {
                ResetParent(item);
                return Elements.Remove(item);
            }

            public void RemoveAt(int index)
            {
                ResetParent(Elements[index]);
                Elements.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Elements.GetEnumerator();
            }

            void SetParent(UIElement element)
            {
                element.Parent = Parent;
                element.Root = Parent.Root;
                if (Parent.Root is not null)
                    element.Recalculate();
            }

            void ResetParent(UIElement element)
            {
                element.Parent = null;
                element.Root = null!;
            }
        }
    }
}
