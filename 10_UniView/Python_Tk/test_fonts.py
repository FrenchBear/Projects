import tkinter as tk
from tkinter import font

def display_text(root, text):
    label = tk.Label(root, text=text)
    label.pack(padx=20, pady=20)

def display_text_with_specific_font(root, text, font_name):
    try:
        custom_font = font.Font(family=font_name)
        label = tk.Label(root, text=text, font=custom_font)
        label.pack(padx=20, pady=20)
    except tk.TclError:
        print(f"Font '{font_name}' not found. Using default font.")
        display_text(root, text) # Fallback to default

if __name__ == "__main__":
    root = tk.Tk()
    root.title("Font Fallback Example")

    # Text containing characters that might not be in all fonts
    complex_text = "Hello 世界! This includes some Unicode characters: éàçüö and Aé♫山𝄞🐗 \uD834\uDD1E"

    print("Using default Tkinter font:")
    display_text(root, complex_text)

    print("\nTrying a specific font (if available):")
    display_text_with_specific_font(root, complex_text, "Segoe UI Symbol") # Likely to have wide coverage

    print("\nTrying Courier New:")
    display_text_with_specific_font(root, complex_text, "Courier New")

    print("\nTrying a font that might not be available:")
    display_text_with_specific_font(root, complex_text, "NonExistentFont")

    root.mainloop()