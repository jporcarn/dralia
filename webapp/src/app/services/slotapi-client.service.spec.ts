import { TestBed } from '@angular/core/testing';

import { SlotapiClientService } from './slotapi-client.service';

describe('SlotapiClientService', () => {
  let service: SlotapiClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SlotapiClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
