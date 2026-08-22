import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-marketing-footer',
  imports: [RouterLink],
  templateUrl: './marketing-footer.component.html',
  styleUrl: './marketing-footer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MarketingFooter {
  readonly currentYear = new Date().getFullYear();
}
