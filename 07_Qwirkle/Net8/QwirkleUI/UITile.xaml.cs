﻿// UITile User Control
// Visual representation of a Qwirkle tile
//
// 2023-12-11   PV

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static QwirkleUI.App;

namespace QwirkleUI;

public partial class UITile: UserControl
{
    public UITile(string ShapeColor, int Instance)
    {
        InitializeComponent();
        this.ShapeColor = ShapeColor;
        this.Instance = Instance;
    }

    private bool _GrayBackground = false;

    public bool GrayBackground
    {
        get => _GrayBackground;
        set
        {
            if (_GrayBackground != value)
            {
                _GrayBackground = value;
                TileLayer.Fill = Resources[value ? "GrayTile" : "BlackTile"] as Brush;
            }
        }
    }

    private bool _SelectionBorder = false;

    public bool SelectionBorder
    {
        get => _SelectionBorder;
        set
        {
            if (_SelectionBorder != value)
            {
                _SelectionBorder = value;
                SelectionLayer.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    private bool _Hatched = false;

    public bool Hatched
    {
        get => _Hatched;
        set
        {
            if (_Hatched != value)
            {
                _Hatched = value;
                HatchLayer.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

    private string _ShapeColor = "";
    public string ShapeColor
    {
        get => _ShapeColor;
        init
        {
            _ShapeColor = value;
            ShapeColorLayer.Fill = Resources[_ShapeColor] as Brush;
        }
    }

    private int _Instance;
    public int Instance
    {
        get => _Instance;
        init => _Instance = value;
    }
}