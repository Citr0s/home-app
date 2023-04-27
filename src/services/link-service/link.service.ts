import { Injectable } from '@angular/core';
import { ILink } from './types/link.type';
import { Observable, of, tap } from 'rxjs';
import { LinkRepository } from './link.repository';
import { LocationMapper } from '../location-service/location.mapper';

@Injectable()
export class LinkService {

    private _linkRepository: LinkRepository;
    private _cachedLinks: Array<ILink> | null = null;

    constructor(linkRepository: LinkRepository) {
        this._linkRepository = linkRepository;
    }

    public refreshCache(): Observable<Array<ILink>> {
        return this._linkRepository.getAllLinks()
            .pipe(
                tap((links) => {
                    this._cachedLinks = links;
                    localStorage.setItem('cachedLinks', JSON.stringify(links));
                })
            );
    }

    public getAllLinks(): Observable<Array<ILink>> {

        if (localStorage.getItem('cachedLinks')) {
            this._cachedLinks = JSON.parse(`${localStorage.getItem('cachedLinks')}`);
        }

        if (this._cachedLinks !== null) {
            return of(this._cachedLinks);
        }

        return this.refreshCache();
    }

    public addLink(link: ILink): Observable<ILink> {
        return this._linkRepository.addLink(link);
    }

    public importLinks(links: Array<ILink>): Observable<Array<ILink>> {
        return this._linkRepository.importLinks(links);
    }

    public updateLink(link: ILink): Observable<ILink> {
        return this._linkRepository.updateLink(link);
    }

    public deleteLink(identifier: string): Observable<any> {
        return this._linkRepository.deleteLink(identifier);
    }

    public getMediaLinks(): Observable<Array<ILink>> {
        return of(this._cachedLinks?.filter((link) => link.category === 'media') ?? []);
    }

    public getSystemLinks(): Observable<Array<ILink>> {
        return of(this._cachedLinks?.filter((link) => link.category === 'system') ?? []);
    }

    public getProductivityLinks(): Observable<Array<ILink>> {
        return of(this._cachedLinks?.filter((link) => link.category === 'productivity') ?? []);
    }

    public getToolsLinks(): Observable<Array<ILink>> {
        return of(this._cachedLinks?.filter((link) => link.category === 'tools') ?? []);
    }
}
