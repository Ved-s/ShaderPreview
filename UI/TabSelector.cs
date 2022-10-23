using Microsoft.Xna.Framework;
using ShaderPreview.UI.Elements;
using ShaderPreview.UI.Helpers;
using System.Collections;
using System.Collections.Generic;

namespace ShaderPreview.UI
{
    public class TabSelector : UIFlow, IElementListController
    {
        public static readonly ElementEvent<Tab, TabSelector> TabSelectedEvent = new();

        private bool ModifyingElements = false;
        private RadioButtonGroup RadioGroup = new();

        public TabCollection Tabs { get; }

        public TabSelector()
        {
            Height = 0;
            ElementSpacing = 2;

            Tabs = new(this);

            RadioGroup.ButtonClicked += GroupButtonClicked;
        }

        private void GroupButtonClicked(UIButton? button, object? tag)
        {
            Tab tab = new(button?.Text ?? "", button?.Selected ?? false, button?.RadioTag);
            Events.Call(TabSelectedEvent, tab);
        }

        private void AddTab(Tab tab)
        {
            UIButton btn = new()
            {
                Width = 0,
                Height = 0,
                Text = tab.Text,
                RadioTag = tab.Tag,
                Selected = tab.Selected,
                RadioGroup = RadioGroup,

                SelectedTextColor = new(.1f, .1f, .1f),
                SelectedBackColor = Color.White,
            };
            ModifyingElements = true;
            Elements.Add(btn);
            ModifyingElements = false;
        }

        private void ClearTabs()
        {
            ModifyingElements = true;
            Elements.Clear();
            RadioGroup.ButtonClicked -= GroupButtonClicked;
            RadioGroup = new();
            RadioGroup.ButtonClicked += GroupButtonClicked;
            ModifyingElements = false;
        }

        bool IElementListController.CanModifyElements() => ModifyingElements;

        public record struct Tab(string Text, bool Selected, object? Tag);
        public class TabCollection : IEnumerable<Tab>
        {
            private readonly TabSelector Parent;

            internal TabCollection(TabSelector parent)
            {
                Parent = parent;
            }

            public void Add(Tab tab)
            {
                Parent.AddTab(tab);
            }

            public void Clear()
            {
                Parent.ClearTabs();
            }

            public IEnumerator<Tab> GetEnumerator()
            {
                foreach (UIElement element in Parent.Elements)
                {
                    if (element is not UIButton button || button.RadioGroup is null)
                        continue;

                    yield return new(button.Text ?? "", button.Selected, button.RadioTag);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
