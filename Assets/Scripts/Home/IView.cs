using System;

namespace Home
{
    public interface IView
    {
        void OnOpening();
        void OnClosing();
    }
}