import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { MarketingFooter } from '../marketing-footer/marketing-footer.component';
import { MarketingHeader } from '../marketing-header/marketing-header.component';

@Component({
  selector: 'app-marketing-layout',
  imports: [MarketingFooter, MarketingHeader, RouterOutlet],
  templateUrl: './marketing-layout.component.html',
  styleUrl: './marketing-layout.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MarketingLayout {}
