﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.WootzJs;
using WootzJs.Models;
using WootzJs.Mvc.Views.Css;
using WootzJs.Web;

namespace WootzJs.Mvc.Views
{
    public class Control : IDisposable, IEnumerable<Control>
    {
        private static MouseTrackingEngine mouseTrackingEngine = new MouseTrackingEngine();

        static Control()
        {
            mouseTrackingEngine.Initialize();
        }

        public Control Parent { get; private set; }
        public Control Label { get; set; }
        public event Action AttachedToDom;
        public event Action DetachedFromDom;
        public event Action<ValidateEvent> Validate;

        protected string TagName { get; set; }

        private Element node;
        private List<Control> children = new List<Control>();
        private Style style;
        private Action<Event> click;
        private Action mouseEntered;
        private Action mouseExited;
        private Action mouseDown;
        private Action mouseUp;
        private Action<KeyboardEvent> keyPress;
        private Action<KeyboardEvent> keyUp;
        private Action<KeyboardEvent> keyDown;
        private Action<WheelEvent> wheel;
        private Action blurred;
        private Action focused;
        private bool isAttachedToDom;
        private View view;
        private bool isDisposed;

        public Control() : this("div")
        {
        }

        public Control(string tagName = "div")
        {
            TagName = tagName;
        }

        public Control(Element node)
        {
            Node = node;
            isAttachedToDom = node.IsAttachedToDom();
        }

        public View View
        {
            get
            {
                if (view != null)
                    return view;
                else if (Parent != null)
                    return Parent.View;
                else
                    return null;
            }
            internal set { view = value; }
        }

        public UrlHelper Url
        {
            get
            {
                var viewContext = ViewContext;
                if (viewContext != null)
                    return viewContext.Url;
                else
                    return MvcApplication.Instance.Url;
            }
        }

        public ViewContext ViewContext
        {
            get
            {
                if (View != null)
                    return View.ViewContext;
                else if (Parent != null)
                    return Parent.ViewContext;
                else
                    return null;
            }
        }

        public Style Style 
        {
            get
            {
                if (style == null)
                    Style = new Style();
                return style;
            }
            set
            {
                style = value;
                style.Attach(Node.Style);
            }
        }

        public Element Node
        {
            get { return EnsureNodeExists(); }
            private set
            {
                node = value;
                node.As<JsObject>().memberset("$control", this.As<JsObject>());
            }
        }

        public MvcApplication Application
        {
            get { return MvcApplication.Instance; }
        }

        public static Control GetControlForElement(Element element)
        {
            return element.As<JsObject>().member("$control").As<Control>();
        }

        protected virtual Element CreateNode()
        {
            return Browser.Document.CreateElement(TagName);
        }

        protected Element EnsureNodeExists()
        {
            if (node == null)
            {
                Node = CreateNode();
                node.SetAttribute("data-class-name", GetType().FullName);
                if (style != null)
                    style.Attach(node.Style);
            }
            return node;
        }

        public Control this[int index]
        {
            get { return children[index]; }
        }

        public IEnumerable<Control> Children
        {
            get { return children; }
        }

        protected virtual void AddChild(Control child)
        {
            if (child == null)
                throw new ArgumentNullException("child");
            if (child.Parent == this)
                throw new Exception("The speciifed child is already present in this container");
            children.Add(child);
            child.Parent = this;
            EnsureNodeExists();
            child.OnAdded();
        }

        protected virtual void RemoveChild(Control child)
        {
            if (child.Parent != this)
                throw new Exception("The specified child is not contained in this container");

            children.Remove(child);
            child.Parent = null;

            OnRemove(child);
        }

        protected virtual void OnRemove(Control child)
        {
            child.OnRemoved();            
        }

        protected void RemoveAll()
        {
            while (Count > 0)
                RemoveChild(this[0]);
        }

        public int Count
        {
            get { return children.Count; }
        }

        public bool IsAttachedToDom
        {
            get { return isAttachedToDom; }
        }

        protected virtual void OnAdded()
        {
            if (Parent.isAttachedToDom)
                OnAttachedToDom();
        }

        protected virtual void OnRemoved()
        {
            OnDetachedFromDom();
        }

        public event Action<Event> Click
        {
            add
            {
                if (click == null)
                    Node.AddEventListener("click", OnJsClick);
                click = (Action<Event>)Delegate.Combine(click, value);
            }
            remove
            {
                click = (Action<Event>)Delegate.Remove(click, value);
                if (click == null)
                    Node.RemoveEventListener("click", OnJsClick);
            }
        }

        public event Action MouseEntered
        {
            add
            {
                if (mouseEntered == null)
                    Node.AddEventListener("mouseentered", OnJsMouseEnter);
                mouseEntered = (Action)Delegate.Combine(mouseEntered, value);
            }
            remove
            {
                mouseEntered = (Action)Delegate.Remove(mouseEntered, value);
                if (mouseEntered == null)
                    Node.RemoveEventListener("mouseentered", OnJsMouseEnter);
            }
        }

        public event Action MouseExited
        {
            add
            {
                if (mouseExited == null)
                    Node.AddEventListener("mouseexited", OnJsMouseLeave);
                mouseExited = (Action)Delegate.Combine(mouseExited, value);
            }
            remove
            {
                mouseExited = (Action)Delegate.Remove(mouseExited, value);
                if (mouseExited == null)
                    Node.RemoveEventListener("mouseexited", OnJsMouseLeave);
            }
        }

        public event Action MouseDown
        {
            add
            {
                if (mouseDown == null)
                    Node.AddEventListener("mousedown", OnJsMouseDown);
                mouseDown = (Action)Delegate.Combine(mouseDown, value);
            }
            remove
            {
                mouseDown = (Action)Delegate.Remove(mouseDown, value);
                if (mouseDown == null)
                    Node.RemoveEventListener("mousedown", OnJsMouseDown);
            }
        }

        public event Action MouseUp
        {
            add
            {
                if (mouseUp == null)
                    Node.AddEventListener("mouseup", OnJsMouseUp);
                mouseUp = (Action)Delegate.Combine(mouseUp, value);
            }
            remove
            {
                mouseUp = (Action)Delegate.Remove(mouseUp, value);
                if (mouseUp == null)
                    Node.RemoveEventListener("mouseup", OnJsMouseUp);
            }
        }

        public event Action<WheelEvent> Wheel
        {
            add
            {
                if (wheel == null)
                    Node.AddEventListener("wheel", OnJsWheel);
                wheel = (Action<WheelEvent>)Delegate.Combine(wheel, value);
            }
            remove
            {
                wheel = (Action<WheelEvent>)Delegate.Remove(wheel, value);
                if (wheel == null)
                    Node.RemoveEventListener("wheel", OnJsWheel);
            }
        }

        public string Hint
        {
            get { return Node.GetAttribute("title"); }
            set { Node.SetAttribute("title", value); }
        }

        private void OnJsClick(Event evt)
        {
            OnClick(evt);
        }

        private void OnJsMouseEnter(Event evt)
        {
            OnMouseEnter();
        }

        private void OnJsMouseLeave(Event evt)
        {
            OnMouseLeave();
        }

        private void OnJsMouseDown(Event evt)
        {
            OnMouseDown();
        }

        private void OnJsMouseUp(Event evt)
        {
            OnMouseUp();
        }

        private void OnJsWheel(Event evt)
        {
            OnWheel(evt.As<WheelEvent>());
        }

        /// <summary>
        /// Warning:  This method will not be invoked if there are no click events attached to it.
        /// </summary>
        protected virtual void OnClick(Event evt)
        {
            var click = this.click;
            if (click != null)
                click(evt);
        }

        public bool IsMouseInControl()
        {
            return Node.IsMouseInElement();
        }

        private void OnMouseEnter()
        {
            if (mouseEntered != null)
                mouseEntered();
        }

        private void OnMouseLeave()
        {
            if (mouseExited != null)
                mouseExited();
        }

        private void OnMouseDown()
        {
            if (mouseDown != null)
                mouseDown();
        }

        private void OnMouseUp()
        {
            if (mouseUp != null)
                mouseUp();
        }

        private void OnWheel(WheelEvent evt)
        {
            if (wheel != null)
                wheel(evt);
        }

        public static implicit operator Control(string text)
        {
            return new Text(text);
        }

        protected virtual void OnAttachedToDom()
        {
            if (!isAttachedToDom)
            {
                isAttachedToDom = true;
                if (AttachedToDom != null)
                    AttachedToDom();

                foreach (var child in Children)
                {
                    child.OnAttachedToDom();
                }                
            }
        }

        protected virtual void OnDetachedFromDom()
        {
            if (isAttachedToDom)
            {
                isAttachedToDom = false;
                if (DetachedFromDom != null)
                    DetachedFromDom();

                foreach (var child in Children)
                {
                    child.OnDetachedFromDom();
                }                
            }
        }

        protected virtual void OnAddedToView()
        {
            foreach (var child in Children)
            {
                child.View = View;
                child.OnAddedToView();
            }
        }

        protected virtual void OnRemovedFromView()
        {
            foreach (var child in Children)
            {
                child.View = null;
                child.OnRemovedFromView();
            }
        }

        internal void NotifyOnAddedToView()
        {
            OnAddedToView();
        }

        internal void NotifyOnRemovedFromView()
        {
            OnRemovedFromView();
        }

/*
        public bool ValidateControl()
        {
            var evt = new ValidateEvent();
            ValidateControlTree(evt);
            ViewContext.ControllerContext.Application.NotifyOnValidate(evt);
            return evt.Validations.All(x => x.IsValid);
        }
*/

/*
        private void ValidateControlTree(ValidateEvent evt)
        {
            OnValidate(evt);
            foreach (var child in Children)
                child.ValidateControlTree(evt);
        }
*/

        protected virtual void OnValidate(ValidateEvent evt)
        {
            var validate = Validate;
            if (validate != null)
            {
                validate(evt);
            }
        }

        public void Dispose()
        {
            Dispose(isDisposed);
            if (isDisposed)
            {
                foreach (var child in Children)
                {
                    child.Dispose();
                }
            }
            isDisposed = true;
        }

        protected virtual void Dispose(bool isDisposed)
        {
        }

        /// <summary>
        /// Helper method that wraps Browser.Document.CreateElement
        /// </summary>
        /// <param name="tagName">The tag name of the HTML element.</param>
        /// <returns>The browser element node</returns>
        protected Element CreateElement(string tagName)
        {
            return Browser.Document.CreateElement(tagName);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Control> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        public void Focus()
        {
            Node.Focus();
        }

        private void OnBlurred(Event evt)
        {
            if (blurred != null)
                blurred();
        }

        public event Action Blurred
        {
            add
            {
                if (blurred == null)
                    Node.AddEventListener("blur", OnBlurred);
                blurred = (Action)Delegate.Combine(blurred, value);
            }
            remove
            {
                blurred = (Action)Delegate.Remove(blurred, value);
                if (blurred == null)
                    Node.RemoveEventListener("blur", OnBlurred);
            }
        }

        private void OnFocused(Event evt)
        {
            if (focused != null)
                focused();
        }

        public event Action Focused
        {
            add
            {
                if (focused == null)
                    Node.AddEventListener("focus", OnFocused);
                focused = (Action)Delegate.Combine(focused, value);
            }
            remove
            {
                focused = (Action)Delegate.Remove(focused, value);
                if (focused == null)
                    Node.RemoveEventListener("focus", OnFocused);
            }
        }

        public event Action<KeyboardEvent> KeyPress
        {
            add
            {
                if (keyPress == null)
                    Node.AddEventListener("keypress", OnKeyPress);
                keyPress = (Action<KeyboardEvent>)Delegate.Combine(keyPress, value);
            }
            remove
            {
                keyPress = (Action<KeyboardEvent>)Delegate.Remove(keyPress, value);
                if (keyPress == null)
                    Node.RemoveEventListener("keypress", OnKeyPress);
            }
        }

        private void OnKeyPress(Event evt)
        {
            if (keyPress != null)
                keyPress((KeyboardEvent)evt);
        }

        public event Action<KeyboardEvent> KeyDown
        {
            add
            {
                if (keyDown == null)
                    Node.AddEventListener("keydown", OnKeyDown);
                keyDown = (Action<KeyboardEvent>)Delegate.Combine(keyDown, value);
            }
            remove
            {
                keyDown = (Action<KeyboardEvent>)Delegate.Remove(keyDown, value);
                if (keyDown == null)
                    Node.RemoveEventListener("keydown", OnKeyDown);
            }
        }

        private void OnKeyDown(Event evt)
        {
            if (keyDown != null)
                keyDown((KeyboardEvent)evt);
        }

        public event Action<KeyboardEvent> KeyUp
        {
            add
            {
                if (keyUp == null)
                    Node.AddEventListener("keyup", OnKeyUp);
                keyUp = (Action<KeyboardEvent>)Delegate.Combine(keyUp, value);
            }
            remove
            {
                keyUp = (Action<KeyboardEvent>)Delegate.Remove(keyUp, value);
                if (keyUp == null)
                    Node.RemoveEventListener("keyup", OnKeyUp);
            }
        }

        private void OnKeyUp(Event evt)
        {
            if (keyUp != null)
                keyUp((KeyboardEvent)evt);
        }

        public static bool IsMouseDown
        {
            get { return mouseTrackingEngine.IsMouseDown; }
        }
    }
}