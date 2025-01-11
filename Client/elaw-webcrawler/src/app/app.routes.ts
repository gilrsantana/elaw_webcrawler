import { Routes } from '@angular/router';
import { DashboardComponent } from './views/dashboard/dashboard.component';
import { Component } from '@angular/core';
import { DefaultLayoutComponent } from './layout/default-layout';

export const routes: Routes = [
	{
		path: '',
		redirectTo: 'dashboard',
		pathMatch: 'full'
	},
	{
		path: '',
		component: DefaultLayoutComponent,
		data: {
			title: 'Home'
		},
		children: [
			{
				path: 'dashboard',
				loadChildren: () => import('./views/dashboard/routes').then(m => m.routes)
			}
		]
	}
];
