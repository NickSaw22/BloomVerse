export interface Member {
    id: string;
    dateOfBirth: string;
    imageUrl?: string;
    displayName: string;
    created: string;
    lastActive: string;
    gender: string;
    description: string;
    city: string;
    country: string;
}

export type Photos = Photo[]

export interface Photo {
    id: number
    url: string
    publicId?: any
    member?: any
    memberId: string
}
