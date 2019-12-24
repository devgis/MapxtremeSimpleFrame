using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MapInfo.Mapping;
using MapInfo.Engine;
using MapInfo.Data;
using MapInfo.Geometry;
using MapInfo.Styles;
using System.IO;

namespace Devgis.EagleEye
{
    public partial class MainMap : Form
    {
        FeatureLayer eagleEye;
        Feature fRec;

        public MainMap()
        {
            InitializeComponent();
            mapControl1.Map.ViewChangedEvent += new MapInfo.Mapping.ViewChangedEventHandler(Map_ViewChangedEvent);
            Map_ViewChangedEvent(this, null);
            this.mapControl2.MouseWheelSupport = new MouseWheelSupport(MouseWheelBehavior.None, 10, 5);
        }

        void Map_ViewChangedEvent(object sender, MapInfo.Mapping.ViewChangedEventArgs e)
        {
            // Display the zoom level
            Double dblZoom = System.Convert.ToDouble(String.Format("{0:E2}", mapControl1.Map.Zoom.Value));
            if (statusStrip1.Items.Count > 0)
            {
                statusStrip1.Items[0].Text = "缩放: " + dblZoom.ToString() + " " + MapInfo.Geometry.CoordSys.DistanceUnitAbbreviation(mapControl1.Map.Zoom.Unit);
            }

            if (Session.Current.Catalog.GetTable("EagleEyeTemp") == null)
                loadEagleLayer();
            Table tblTemp = Session.Current.Catalog.GetTable("EagleEyeTemp");
            try
            {
                (tblTemp as ITableFeatureCollection).Clear();
            }
            catch (Exception)
            { }

            try
            {
                if (mapControl2.Map.Layers["MyEagleEye"] != null)
                    mapControl2.Map.Layers.Remove(eagleEye);
                DRect rect = mapControl1.Map.Bounds;
                FeatureGeometry fg = new MapInfo.Geometry.Rectangle(mapControl1.Map.GetDisplayCoordSys(), rect);
                SimpleLineStyle vLine = new SimpleLineStyle(new LineWidth(2, LineWidthUnit.Point), 2, Color.Red);
                SimpleInterior vInter = new SimpleInterior(9, Color.Yellow, Color.Yellow, true);
                CompositeStyle cStyle = new CompositeStyle(new AreaStyle(vLine, vInter), null, null, null);
                fRec = new Feature(fg, cStyle);
                tblTemp.InsertFeature(fRec);
                eagleEye = new FeatureLayer(tblTemp, "EagleEye ", "MyEagleEye");
                mapControl2.Map.Layers.Insert(0, eagleEye);
            }
            catch (Exception)
            { }
        }

        private void mapControl2_MouseClick(object sender, MouseEventArgs e)
        {
            //鹰眼地图点击时切换主地图到该点中兴
            DPoint pt = new DPoint();
            mapControl2.Map.DisplayTransform.FromDisplay(e.Location, out pt);
            mapControl1.Map.Center = pt;

        }

        private void MainMap_Load(object sender, EventArgs e)
        {
            this.LoadMap();
        }

        private void LoadMap()
        {
            string MapPath = Path.Combine(Application.StartupPath, @"Data\World.mws");
            MapWorkSpaceLoader mwsLoader = new MapWorkSpaceLoader(MapPath);
            mapControl1.Map.Load(mwsLoader);
            mapControl2.Map = mapControl1.Map.Clone() as Map;
        }

        private void loadEagleLayer()
        {
            TableInfoMemTable ti = new TableInfoMemTable("EagleEyeTemp");
            ti.Temporary = true;
            Column column;
            column = new GeometryColumn(mapControl2.Map.GetDisplayCoordSys());
            column.Alias = "MI_Geometry ";
            column.DataType = MIDbType.FeatureGeometry;
            ti.Columns.Add(column);

            column = new Column();
            column.Alias = "MI_Style ";
            column.DataType = MIDbType.Style;
            ti.Columns.Add(column);
            Table table;
            try
            {
                table = Session.Current.Catalog.CreateTable(ti);

            }
            catch (Exception ex)
            {
                table = Session.Current.Catalog.GetTable("EagleEyeTemp");
            }
            if (mapControl2.Map.Layers["MyEagleEye"] != null)
                mapControl2.Map.Layers.Remove(eagleEye);
            eagleEye = new FeatureLayer(table, "EagleEye ", "MyEagleEye");
            mapControl2.Map.Layers.Insert(0, eagleEye);
            mapControl1.Refresh();
        }

        private void mapToolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            if ("toolBarEagle".Equals(e.Button.Name))
            {
                pEagle.Visible = !pEagle.Visible;
            }
        }

        private void MainMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.Start("http://flysoft.taobao.com/");
        }
    }
}