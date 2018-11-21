using System;
using System.Collections.Generic;
using System.Text;
using Linq.Attributes;

namespace Linq.Tables
{
    [Table("picture")]
    public class Picture
    {
        [PrimaryKey, Column("id")]
        public int Id { get; set; }
        [Column("filename")]
        public string Filename { get; set; }
        [Column("cameraid")]
        public int? CameraId { get; set; }
        [Column("iptckeywords")]
        public string IptcKeywords { get; set; }
        [Column("iptccopyright")]
        public string IptcCopyright { get; set; }
        [Column("iptcheadline")]
        public string IptcHeadline { get; set; }
        [Column("iptccaption")]
        public string IptcCaption { get; set; }
        [Column("exifaperture")]
        public double? ExifAperture { get; set; }
        [Column("exifexposuretime")]
        public double? ExifExposureTime { get; set; }
        [Column("exifiso")]
        public double? ExifIso { get; set; }
        [Column("exifflash")]
        public bool? ExifFlash { get; set; }
        [Column("exifexposureprog")]
        public short? ExifExposureProg { get; set; }
        [Column("photographerid")]
        public int? PhotographerId { get; set; }

    }
}
