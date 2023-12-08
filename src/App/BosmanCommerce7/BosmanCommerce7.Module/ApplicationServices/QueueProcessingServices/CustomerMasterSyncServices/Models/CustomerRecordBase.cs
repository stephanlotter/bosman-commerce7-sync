/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-08
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models {
  public abstract record CustomerRecordBase {
    public string? Honorific { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? Address { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public string? StateCode { get; init; }

    public string? ZipCode { get; init; }

    public string CountryCode { get; init; } = "ZA";

    public EmailAddress[] Emails { get; init; }

    public TelephoneNumber[] Phones { get; init; }
  }

  public record EmailAddress {
    public string Email { get; init; }
  }

  public record TelephoneNumber {
    public string Phone { get; init; }
  }
}