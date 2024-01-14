type ProjectQuery = 'getAllProjects' | 'getMyProjects' | 'getUnassignedProjects' | 'getArchivedProjects' | 'getProjectsByMember'
type ProjectSort = 'name' | 'startdate' | 'enddate' | 'pm' | 'priority'
type ProjectOrder = 'asc' | 'desc'
type ProjectOptions = {
  container?: HTMLElement,
  page?: number,
  limit?: number,
  sortBy?: ProjectSort,
  order?: ProjectOrder
  memberId?: string,
};

class ProjectsTable {
  options?: ProjectOptions
  query: ProjectQuery
  container: HTMLElement
  page: number
  limit: number
  sortBy: ProjectSort
  order?: ProjectOrder
  memberId?: string

  constructor(
    query?: ProjectQuery,
    options?: ProjectOptions
  ) {
    this.query = query ?? 'getAllProjects',
    this.container = options?.container ?? document.querySelector('[data-container="projects"]') as HTMLElement,
    this.page = options?.page ?? 1,
    this.limit = options?.limit ?? 4,
    this.sortBy = options?.sortBy ?? 'enddate',
    this.order = options?.order ?? 'desc'
  }

  async init() {
    this.getProjects()

    this.container.addEventListener('click', async (event) => {
      const header = (event.target as HTMLElement).closest('[data-sortable]')
      if (header) {
        if (header.getAttribute('data-order') === this.order)
          this.order =
            header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc'

        this.page =
          parseInt(header.getAttribute('data-page') as string) ?? this.page
        this.limit =
          parseInt(header.getAttribute('data-limit') as string) ?? this.limit
        this.sortBy = (header.getAttribute('data-sort') as ProjectSort) ?? this.sortBy

        this.getProjects()
      }
    })
  }

  async getProjects() {
    try {
      const url = `/Projects/GetProjectsByQuery?query=${this.query}&page=${this.page}&sortBy=${this.sortBy}&order=${this.order}&limit=${this.limit}&memberId=${this.memberId}`

      const response = await fetch(url);

      if (!response.ok) {
        throw new Error(`Error loading projects table: ${response.statusText}`)
      }

      const html = await response.text()

      this.container.innerHTML = html

      const header = this.container.querySelector(
        `[data-sort="${this.sortBy}"]`
      ) as HTMLElement

      header.setAttribute('data-order', this.order as ProjectOrder)

      header.classList.add('active')

      const arrow = header.querySelector('.order')

      arrow?.classList.add(this.order as ProjectOrder)
    } catch (error) {
      console.error(error)
    }
  }
}
