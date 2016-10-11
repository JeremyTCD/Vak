import { platformBrowser } from '@angular/platform-browser';
import { AppModuleNgFactory } from './Compiled/Angular/vak-app.module.ngfactory';

platformBrowser().bootstrapModuleFactory(AppModuleNgFactory);
