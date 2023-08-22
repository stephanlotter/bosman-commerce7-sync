/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

public record VersionDescriptor {

  public int Major { get; init; }

  public int Minor { get; init; }

  public int Build { get; init; }

  public int Revision { get; init; }

  public VersionDescriptor(string version) {
    var versionSegments = version.Split(new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);

    int Parse(int versionSegmentIndex) {
      if (versionSegmentIndex >= versionSegments.Length) { return 0; }
      var versionValue = versionSegments[versionSegmentIndex];
      if (string.IsNullOrEmpty(versionValue)) { return 0; }
      if (int.TryParse(versionValue, out var i)) { return i; }
      return 0;
    }

    Major = Parse(0);
    Minor = Parse(1);
    Build = Parse(2);
    Revision = Parse(3);
  }

  public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";

}